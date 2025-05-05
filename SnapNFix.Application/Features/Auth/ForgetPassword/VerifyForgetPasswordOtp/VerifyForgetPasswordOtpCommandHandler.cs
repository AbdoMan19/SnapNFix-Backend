using MediatR;
using Microsoft.AspNetCore.Http;
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
    
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VerifyForgetPasswordOtpCommandHandler(IUserService userService,
     ILogger<VerifyForgetPasswordOtpCommandHandler> logger, IOtpService otpService, ITokenService tokenService, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _logger = logger;
        _otpService = otpService;
        _tokenService = tokenService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponseModel<string>> Handle(VerifyForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        
        if (user == null || user.IsDeleted || user.IsSuspended)
        {
            _logger.LogWarning("User not found for OTP verification attempt");
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.Otp), "User not found")
            });
        }

        var otpVerificationResult = await _otpService.VerifyOtpAsync(user.PhoneNumber ?? user.Email, request.Otp, OtpPurpose.ForgotPassword);

        if (!otpVerificationResult)
        {
            _logger.LogWarning("OTP verification failed for user {UserId}", user.Id);
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.Otp), "Invalid OTP")
            });
        }

        var token = await _tokenService.GeneratePasswordResetToken(user);
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Failed to generate reset password token for user {UserId}", user.Id);
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.Otp), "Failed to generate reset password token")
            });
        }
        
        _logger.LogInformation("OTP verified successfully for user {UserId}", user.Id);
        return GenericResponseModel<string>.Success(token);
    }
}