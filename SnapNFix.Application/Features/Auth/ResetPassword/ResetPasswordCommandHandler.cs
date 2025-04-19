using MediatR;
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

    public ResetPasswordCommandHandler(UserManager<User> userManager, IUserService userService, ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _userService = userService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create(nameof(request.EmailOrPhoneNumber), "Invalid Email or Phone Number")
        };
        (var isEmail , var isPhone , var user) = await _userService.GetUserByEmailOrPhoneNumber(request.EmailOrPhoneNumber);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: User not found for identifier {Identifier}", nameof(request.EmailOrPhoneNumber));
            return GenericResponseModel<bool>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
            _logger.LogWarning("Reset password failed for user {UserId} with {ErrorCount} errors", user.Id, errors.Count);
            return GenericResponseModel<bool>.Failure("Reset password failed", errors);
        }

        if (isEmail) ;
        if (isPhone) ;

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
        return GenericResponseModel<bool>.Success(true);
    }
}