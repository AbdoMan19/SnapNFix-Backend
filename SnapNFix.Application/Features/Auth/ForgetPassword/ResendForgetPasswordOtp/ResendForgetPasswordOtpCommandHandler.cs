using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services.UserValidationServices;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.ResendForgetPasswordOtp;

public class ResendForgetPasswordOtpCommandHandler : IRequestHandler<ResendForgetPasswordOtpCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly ILogger<ResendForgetPasswordOtpCommandHandler> _logger;
    private readonly IUserValidationService _userValidationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResendForgetPasswordOtpCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        ILogger<ResendForgetPasswordOtpCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        IUserValidationService userValidationService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _userValidationService = userValidationService;
    }

    public async Task<GenericResponseModel<bool>> Handle(ResendForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var contactClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "contact")?.Value;

        if (string.IsNullOrEmpty(contactClaim))
        {
            _logger.LogWarning("Contact number not found in the request");
            return GenericResponseModel<bool>.Failure(Shared.ContactNotFound);
        }

        await _otpService.InvalidateOtpAsync(contactClaim, OtpPurpose.ForgotPassword);

        var otp = await _otpService.GenerateOtpAsync(contactClaim, OtpPurpose.ForgotPassword);

        // TODO : Send OTP to user via SMS

        _logger.LogInformation("OTP sent successfully to {ContactClaim}", contactClaim);
        return GenericResponseModel<bool>.Success(true);
    }
}
