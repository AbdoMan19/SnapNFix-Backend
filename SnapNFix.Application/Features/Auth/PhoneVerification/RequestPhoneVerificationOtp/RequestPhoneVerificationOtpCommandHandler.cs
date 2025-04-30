using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.RequestPhoneVerificationOtp;

public class RequestPhoneVerificationOtpCommandHandler : IRequestHandler<RequestPhoneVerificationOtpCommand, GenericResponseModel<string>>
{
    private readonly IOtpService _otpService;
    private readonly ILogger<RequestPhoneVerificationOtpCommandHandler> _logger;
    private readonly ITokenService _tokenService;
    
    // Uncomment when ready to use SMS service
  // private readonly ISmsService _smsService;

  public RequestPhoneVerificationOtpCommandHandler(
      IOtpService otpService,
      IUnitOfWork unitOfWork,
      ILogger<RequestPhoneVerificationOtpCommandHandler> logger,
      ITokenService tokenService
      // ISmsService smsService
  )
  {
    _otpService = otpService;
    _logger = logger;
    _tokenService = tokenService;
    // _smsService = smsService;
  }

    public async Task<GenericResponseModel<string>> Handle(RequestPhoneVerificationOtpCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing phone verification OTP request for {PhoneNumber}", request.PhoneNumber);
        
        //check if the phone number already exists
        
        
        
        var otp = await _otpService.GenerateOtpAsync(request.PhoneNumber, OtpPurpose.PhoneVerification);
        _logger.LogInformation("Generated OTP for phone number {PhoneNumber}", request.PhoneNumber);


        // Uncomment when ready to actually send SMS
        // var isSmsSent = await _smsService.SendSmsAsync(request.PhoneNumber, otp);
        // if (!isSmsSent)
        // {
        //     _logger.LogWarning("Failed to send OTP to phone number {PhoneNumber}", request.PhoneNumber);
        //     return GenericResponseModel<string>.Failure("Failed to send OTP. Please try again later.");
        // }
        
        string token = await _tokenService.GenerateOtpRequestToken(request.PhoneNumber);
        _logger.LogInformation("Generated OTP request token for phone number {PhoneNumber}", request.PhoneNumber);
        return GenericResponseModel<string>.Success(token);
    }
}