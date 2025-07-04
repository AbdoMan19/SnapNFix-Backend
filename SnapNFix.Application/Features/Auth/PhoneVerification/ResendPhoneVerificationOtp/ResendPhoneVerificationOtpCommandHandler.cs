using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Interfaces;

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
        var contactClaim = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == "contact")?.Value;

        if (string.IsNullOrEmpty(contactClaim))
        {
            _logger.LogWarning("Contact number not found in the request token");
            return GenericResponseModel<bool>.Failure(Shared.ContactNotFound,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(contactClaim), Shared.ContactNotFound)
                });
        }

        try
        {
            await _otpService.InvalidateOtpAsync(contactClaim, OtpPurpose.PhoneVerification);
            var otp = await _otpService.GenerateOtpAsync(contactClaim, OtpPurpose.PhoneVerification);

            _logger.LogInformation("Verification OTP resent successfully to {ContactClaim}", contactClaim);
            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP to {ContactClaim}", contactClaim);
            return GenericResponseModel<bool>.Failure(
                Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(contactClaim), Shared.OtpFailedToSend)
                });
        }
    }
}