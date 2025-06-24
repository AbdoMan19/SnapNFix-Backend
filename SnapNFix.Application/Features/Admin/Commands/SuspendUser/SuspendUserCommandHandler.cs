using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Commands.SuspendUser;


public class SuspendUserQueryHandler : IRequestHandler<SuspendUserQuery, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SuspendUserQueryHandler> _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly ITokenService _tokenService;

    public SuspendUserQueryHandler(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<SuspendUserQueryHandler> logger,
        ICacheInvalidationService cacheInvalidationService,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _cacheInvalidationService = cacheInvalidationService;
        _tokenService = tokenService;
    }

    public async Task<GenericResponseModel<bool>> Handle(SuspendUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user suspension request for UserId: {UserId}, Suspend: {IsSuspended}",
            request.UserId, request.IsSuspended);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _unitOfWork.Repository<User>()
                .FindBy(u => u.Id == request.UserId && !u.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found or deleted for UserId: {UserId}", request.UserId);
                return GenericResponseModel<bool>.Failure(
                    Shared.UserNotFound,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.UserId), Shared.UserNotFound)
                    });
            }

            // Check if user is SuperAdmin (cannot suspend SuperAdmin)
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("SuperAdmin"))
            {
                _logger.LogWarning("Attempt to suspend SuperAdmin user: {UserId}", request.UserId);
                return GenericResponseModel<bool>.Failure(
                    "Cannot suspend SuperAdmin users",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.UserId), "SuperAdmin users cannot be suspended")
                    });
            }

            if (request.IsSuspended)
            {
                user.AccessFailedCount = 3;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                var userDevices = await _unitOfWork.Repository<UserDevice>()
                    .FindBy(d => d.UserId == request.UserId)
                    .Include(d => d.RefreshToken)
                    .ToListAsync(cancellationToken);

                foreach (var device in userDevices.Where(d => d.RefreshToken != null && !d.RefreshToken.IsExpired))
                {
                    device.RefreshToken.Expires = DateTime.UtcNow;
                    await _unitOfWork.Repository<RefreshToken>().Update(device.RefreshToken);
                }

            }
            else
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;

                _logger.LogInformation("User {UserId} unsuspended successfully", request.UserId);
            }

            await _unitOfWork.Repository<User>().Update(user);
            
            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);

            await _cacheInvalidationService.InvalidateUserCacheAsync(request.UserId);

            _logger.LogInformation("User {UserId} {Action} successfully", request.UserId, request.IsSuspended ? "suspended" : "unsuspended");

            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error processing user suspension for UserId: {UserId}", request.UserId);
            return GenericResponseModel<bool>.Failure(Shared.OperationFailed);
        }
    }
}