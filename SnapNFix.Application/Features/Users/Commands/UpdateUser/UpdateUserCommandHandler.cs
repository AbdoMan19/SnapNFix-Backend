using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand , GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    
    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IUserService userService,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            var user = await _unitOfWork.Repository<User>()
                .FindBy(u => u.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return GenericResponseModel<bool>.Failure("User not found");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                bool hasChanges = false;

                // Update only if values are different and provided
                if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != user.FirstName)
                {
                    user.FirstName = request.FirstName.Trim();
                    hasChanges = true;
                }

                if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName != user.LastName)
                {
                    user.LastName = request.LastName.Trim();
                    hasChanges = true;
                }

                if (request.Gender.HasValue && request.Gender != user.Gender)
                {
                    user.Gender = request.Gender.Value;
                    hasChanges = true;
                }

                if (request.BirthDate.HasValue && request.BirthDate != user.BirthDate)
                {
                    user.BirthDate = request.BirthDate.Value;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    _logger.LogInformation("No changes detected for user {UserId}", userId);
                    return GenericResponseModel<bool>.Success(true);
                }

                // Set update timestamp
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Repository<User>().Update(user);
                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("User {UserId} updated successfully", userId);
                return GenericResponseModel<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating user with ID {UserId}", userId);
                return GenericResponseModel<bool>.Failure("An error occurred while updating the user");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in updating user");
            return GenericResponseModel<bool>.Failure("An unexpected error occurred");
        }
    }

}