using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class VerifyForgetPasswordOtpCommandHandler : IRequestHandler<VerifyForgetPasswordOtpCommand, GenericResponseModel<string>>
{
    private readonly IUserService _userService;
    private readonly ILogger<VerifyForgetPasswordOtpCommandHandler> _logger;
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;

    public VerifyForgetPasswordOtpCommandHandler(IUserService userService , ILogger<VerifyForgetPasswordOtpCommandHandler> logger , IOtpService otpService , ITokenService tokenService , UserManager<User> userManager)
    {
        _userService = userService;
        _logger = logger;
        _otpService = otpService;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public async Task<GenericResponseModel<string>> Handle(VerifyForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create(nameof(request.EmailOrPhoneNumber), "Invalid Email or Phone Number")
        };
        var invalidOtpError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create(nameof(request.EmailOrPhoneNumber), "Invalid OTP")
        };
        (var isEmail , var isPhone , var user) = await _userService.GetUserByEmailOrPhoneNumber(request.EmailOrPhoneNumber);
        if (user == null)
        {
            _logger.LogWarning("Verify ForgetPassword Otp attempt failed: User not found for identifier {Identifier}", nameof(request.EmailOrPhoneNumber));
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        var isOtpValid = await _otpService.VerifyOtpAsync(request.EmailOrPhoneNumber, request.Otp, OtpPurpose.ForgotPassword);
        if(!isOtpValid)
        {
            _logger.LogWarning("Verify ForgetPassword Otp attempt failed: Invalid OTP for identifier {Identifier}", nameof(request.EmailOrPhoneNumber));
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, invalidOtpError);
        }
        await _otpService.InvalidateOtpAsync(request.EmailOrPhoneNumber, OtpPurpose.ForgotPassword);
        await _userManager.UpdateSecurityStampAsync(user);
        var token = await _tokenService.GeneratePasswordResetToken(user);
        return GenericResponseModel<string>.Success(token);
    }
}