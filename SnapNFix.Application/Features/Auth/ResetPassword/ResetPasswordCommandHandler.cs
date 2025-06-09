using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand , GenericResponseModel<
bool>>
{
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResetPasswordCommandHandler(UserManager<User> userManager, IUserService userService, ILogger<ResetPasswordCommandHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _userService = userService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponseModel<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var contactClaim = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == "contact")?.Value;

        if (string.IsNullOrEmpty(contactClaim))
        {
            _logger.LogWarning("Contact number not found in the request");
            return GenericResponseModel<bool>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.NewPassword), "Contact number not found")
            });
        }
        var user = await _userManager.FindByNameAsync(contactClaim);
        if (user == null || user.IsDeleted || user.IsSuspended)
        {
            _logger.LogWarning("User not found for password reset attempt");
            return GenericResponseModel<bool>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.NewPassword), "User not found")
            });
        }

        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.NewPassword);
        await _userManager.UpdateSecurityStampAsync(user);
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Reset password attempt failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return GenericResponseModel<bool>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.NewPassword), "Failed to reset password")
            });
        }
        

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
        return GenericResponseModel<bool>.Success(true);
    }
}