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
    private readonly IUnitOfWork _unitOfWork;
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
    _unitOfWork = unitOfWork;
    _logger = logger;
    _tokenService = tokenService;
    // _smsService = smsService;
  }

    public async Task<GenericResponseModel<string>> Handle(RequestPhoneVerificationOtpCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing phone verification OTP request for {PhoneNumber}", request.PhoneNumber);

        var user = await _unitOfWork.Repository<User>()
            .FindBy(u => u.PhoneNumber == request.PhoneNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Phone verification OTP request failed: User not found for {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<string>.Failure("User not found");
        }

        if (user.PhoneNumberConfirmed)
        {
            _logger.LogInformation("Phone number {PhoneNumber} is already verified", request.PhoneNumber);
            return GenericResponseModel<string>.Failure("Phone number is already verified");
        }

        var otp = await _otpService.GenerateOtpAsync(request.PhoneNumber, OtpPurpose.PhoneVerification);
        _logger.LogInformation("Generated OTP for phone number {PhoneNumber}", request.PhoneNumber);

        var phoneNumberVerificationToken = await _tokenService.GeneratePhoneVerificationTokenAsync(user);
        _logger.LogInformation("Generated verification token for user {UserId}", user.Id);

        // Uncomment when ready to actually send SMS
        // var isSmsSent = await _smsService.SendSmsAsync(request.PhoneNumber, otp);
        // if (!isSmsSent)
        // {
        //     _logger.LogWarning("Failed to send OTP to phone number {PhoneNumber}", request.PhoneNumber);
        //     return GenericResponseModel<string>.Failure("Failed to send OTP. Please try again later.");
        // }

        _logger.LogInformation("OTP request processed successfully for phone number {PhoneNumber}", request.PhoneNumber);
        
        return GenericResponseModel<string>.Success(phoneNumberVerificationToken);
    }
}