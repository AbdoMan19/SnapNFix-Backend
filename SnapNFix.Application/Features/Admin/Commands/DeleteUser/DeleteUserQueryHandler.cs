using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Commands.DeleteUser;


public class DeleteUserQueryHandler : IRequestHandler<DeleteUserQuery, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DeleteUserQueryHandler> _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public DeleteUserQueryHandler(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<DeleteUserQueryHandler> logger,
        ICacheInvalidationService cacheInvalidationService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<GenericResponseModel<bool>> Handle(DeleteUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user deletion request for UserId: {UserId}", request.UserId);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _unitOfWork.Repository<User>()
                .FindBy(u => u.Id == request.UserId && !u.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found or already deleted for UserId: {UserId}", request.UserId);
                return GenericResponseModel<bool>.Failure(
                    Shared.UserNotFound,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.UserId), Shared.UserNotFound)
                    });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("SuperAdmin"))
            {
                _logger.LogWarning("Attempt to delete SuperAdmin user: {UserId}", request.UserId);
                return GenericResponseModel<bool>.Failure(
                    "Cannot delete SuperAdmin users",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.UserId), "SuperAdmin users cannot be deleted")
                    });
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            user.AccessFailedCount = 3;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _unitOfWork.Repository<User>().Update(user);

            var userDevices = await _unitOfWork.Repository<UserDevice>()
                .FindBy(d => d.UserId == request.UserId)
                .Include(d => d.RefreshToken)
                .ToListAsync(cancellationToken);

            foreach (var device in userDevices.Where(d => d.RefreshToken != null && !d.RefreshToken.IsExpired))
            {
                device.RefreshToken.Expires = DateTime.UtcNow;
                await _unitOfWork.Repository<RefreshToken>().Update(device.RefreshToken);
            }

            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);

            await _cacheInvalidationService.InvalidateUserCacheAsync(request.UserId);

            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error deleting user with UserId: {UserId}", request.UserId);
            return GenericResponseModel<bool>.Failure(Shared.OperationFailed);
        }
    }
}
