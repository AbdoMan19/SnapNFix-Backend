using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Resources;

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
        var contactClaim = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == "contact")?.Value;

        if (string.IsNullOrEmpty(contactClaim))
        {
            _logger.LogWarning("Contact number not found in the request");
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.Otp), Shared.ContactNotFound)
            });
        }

        var otpVerificationResult = await _otpService.VerifyOtpAsync(contactClaim, request.Otp, OtpPurpose.ForgotPassword);

        if (!otpVerificationResult)
        {
            _logger.LogWarning("OTP verification failed for user {UserId}", contactClaim);
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.Otp), Shared.InvalidOtp)
            });
        }

        var token =  _tokenService.GenerateToken(contactClaim , TokenPurpose.PasswordReset);
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Failed to generate reset password token for user {UserId}", contactClaim);
            return GenericResponseModel<string>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create(nameof(request.Otp), Shared.FailedToGenerateResetToken)
            });
        }
        
        _logger.LogInformation("OTP verified successfully for user {UserId}", contactClaim);
        return GenericResponseModel<string>.Success(token);
    }
}
