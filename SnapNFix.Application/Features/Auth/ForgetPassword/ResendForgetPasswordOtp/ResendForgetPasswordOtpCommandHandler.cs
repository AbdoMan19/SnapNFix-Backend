using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services.UserValidationServices;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.ResendForgetPasswordOtp;

public class ResendForgetPasswordOtpCommandHandler : IRequestHandler<ResendForgetPasswordOtpCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly ILogger<ResendForgetPasswordOtpCommandHandler> _logger;
    
    private readonly IUserValidationService _userValidationService;
    private readonly IHttpContextAccessor httpContextAccessor;

  public ResendForgetPasswordOtpCommandHandler(
  IUnitOfWork unitOfWork,
  IOtpService otpService,
  ILogger<ResendForgetPasswordOtpCommandHandler> logger,
  IHttpContextAccessor httpContextAccessor,
  IUserValidationService userValidationService
  )
  {
    _unitOfWork = unitOfWork;
    _otpService = otpService;
    _logger = logger;
    this.httpContextAccessor = httpContextAccessor;
    _userValidationService = userValidationService;
  }

  public async Task<GenericResponseModel<bool>> Handle(ResendForgetPasswordOtpCommand request, CancellationToken cancellationToken)
  {

    var phoneNumber = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.MobilePhone)?.Value;
    
    if (string.IsNullOrEmpty(phoneNumber))
    {
      _logger.LogWarning("Phone number not found in the request");
      return GenericResponseModel<bool>.Failure("Phone number not found");
    }

    
    await _otpService.InvalidateOtpAsync(phoneNumber, OtpPurpose.ForgotPassword);

    var otp = await _otpService.GenerateOtpAsync(phoneNumber, OtpPurpose.ForgotPassword);

    // TODO : Send OTP to user via SMS
    

    _logger.LogInformation("OTP sent successfully to {PhoneNumber}", phoneNumber);
    return GenericResponseModel<bool>.Success(true);
  }
}