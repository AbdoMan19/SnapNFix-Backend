using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;

public class UpdateAdminProfileCommandHandler : IRequestHandler<UpdateAdminProfileCommand, GenericResponseModel<bool>>
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UpdateAdminProfileCommandHandler> _logger;

    public UpdateAdminProfileCommandHandler(
        IUserService userService,
        UserManager<User> userManager,
        ILogger<UpdateAdminProfileCommandHandler> logger)
    {
        _userService = userService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<GenericResponseModel<bool>> Handle(UpdateAdminProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = await _userService.GetCurrentUserIdAsync();
            
            // Use UserManager to find the user - this ensures proper tracking
            var user = await _userManager.FindByIdAsync(currentUserId.ToString());
                
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Admin with ID {UserId} not found", currentUserId);
                return GenericResponseModel<bool>.Failure(
                    Shared.UserNotFound,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(currentUserId), Shared.UserNotFound)
                    });
            }

            // Verify user has admin or super admin role
            var userRoles = await _userManager.GetRolesAsync(user);
            var hasAdminRole = userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin");
            
            if (!hasAdminRole)
            {
                _logger.LogWarning("User {UserId} attempted to update admin profile without admin privileges", currentUserId);
                return GenericResponseModel<bool>.Failure(
                    Shared.AccessDenied,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authorization", Shared.AccessDenied)
                    });
            }

            bool hasChanges = false;

            // Update First Name if provided and different
            if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName.Trim() != user.FirstName)
            {
                user.FirstName = request.FirstName.Trim();
                hasChanges = true;
                _logger.LogInformation("Updated FirstName for admin {UserId}", currentUserId);
            }

            // Update Last Name if provided and different
            if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName.Trim() != user.LastName)
            {
                user.LastName = request.LastName.Trim();
                hasChanges = true;
                _logger.LogInformation("Updated LastName for admin {UserId}", currentUserId);
            }

            // Update Phone Number if provided and different
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var trimmedPhone = request.PhoneNumber.Trim();
                if (trimmedPhone != user.PhoneNumber)
                {
                    // Check if phone number is already taken by another user using UserManager
                    var existingUserWithPhone = await _userManager.Users
                        .Where(u => u.PhoneNumber == trimmedPhone && u.Id != currentUserId && !u.IsDeleted)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existingUserWithPhone != null)
                    {
                        _logger.LogWarning("Phone number {PhoneNumber} already exists for another user", trimmedPhone);
                        return GenericResponseModel<bool>.Failure(
                            Shared.PhoneAlreadyExists,
                            new List<ErrorResponseModel>
                            {
                                ErrorResponseModel.Create(nameof(request.PhoneNumber), Shared.PhoneAlreadyExists)
                            });
                    }

                    user.PhoneNumber = trimmedPhone;
                    user.UserName = trimmedPhone; 
                    user.NormalizedUserName = trimmedPhone.ToUpper();
                    user.PhoneNumberConfirmed = true; 
                    hasChanges = true;
                    _logger.LogInformation("Updated PhoneNumber for admin {UserId}", currentUserId);
                }
            }

            // Update Gender if provided and different
            if (request.Gender.HasValue && request.Gender != user.Gender)
            {
                user.Gender = request.Gender.Value;
                hasChanges = true;
                _logger.LogInformation("Updated Gender for admin {UserId}", currentUserId);
            }

            // Update Birth Date if provided and different
            if (request.BirthDate.HasValue && request.BirthDate != user.BirthDate)
            {
                user.BirthDate = request.BirthDate.Value;
                hasChanges = true;
                _logger.LogInformation("Updated BirthDate for admin {UserId}", currentUserId);
            }

            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for admin profile update {UserId}", currentUserId);
                return GenericResponseModel<bool>.Success(true);
            }

            user.UpdatedAt = DateTime.UtcNow;

            // Use UserManager.UpdateAsync instead of repository to avoid tracking conflicts
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
                _logger.LogError("Failed to update admin profile for user {UserId}. Errors: {Errors}", 
                    currentUserId, string.Join(", ", errors.Select(e => e.Message)));
                
                return GenericResponseModel<bool>.Failure(Shared.OperationFailed, errors);
            }

            _logger.LogInformation("Admin profile updated successfully for user {UserId}", currentUserId);
            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in admin profile update");
            return GenericResponseModel<bool>.Failure(Shared.UnexpectedError,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request), Shared.UnexpectedError)
                });
        }
    }
}