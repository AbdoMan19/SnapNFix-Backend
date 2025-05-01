using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.ResendPhoneVerificationOtp;

public class ResendPhoneVerificationOtpCommandHandler : IRequestHandler<ResendPhoneVerificationOtpCommand, GenericResponseModel<bool>>
{
    private readonly IOtpService _otpService;
    private readonly ILogger<ResendPhoneVerificationOtpCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResendPhoneVerificationOtpCommandHandler(
        IOtpService otpService,
        ILogger<ResendPhoneVerificationOtpCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _otpService = otpService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponseModel<bool>> Handle(ResendPhoneVerificationOtpCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = _httpContextAccessor.HttpContext?.User.FindFirst("phone")?.Value;
        if (string.IsNullOrEmpty(phoneNumber))
        {
            _logger.LogWarning("Phone number not found in the request token");
            return GenericResponseModel<bool>.Failure("Phone number not found");
        }

        await _otpService.InvalidateOtpAsync(phoneNumber, OtpPurpose.PhoneVerification);

        var otp = await _otpService.GenerateOtpAsync(phoneNumber, OtpPurpose.PhoneVerification);

        // TODO: Send OTP to user via SMS
        // When you implement SMS service, uncomment and use it here
        // await _smsService.SendSmsAsync(phoneNumber, otp, "Your verification code is: {0}");

        _logger.LogInformation("Verification OTP resent successfully to {PhoneNumber}", phoneNumber);
        return GenericResponseModel<bool>.Success(true);
    }
}