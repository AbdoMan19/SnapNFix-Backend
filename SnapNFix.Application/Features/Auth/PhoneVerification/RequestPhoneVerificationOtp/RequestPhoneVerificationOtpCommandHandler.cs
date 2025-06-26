using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.RequestPhoneVerificationOtp;

public class RequestPhoneVerificationOtpCommandHandler : IRequestHandler<RequestPhoneVerificationOtpCommand, GenericResponseModel<string>>
{
    private readonly IOtpService _otpService;
    private readonly ILogger<RequestPhoneVerificationOtpCommandHandler> _logger;
    private readonly ITokenService _tokenService;

    public RequestPhoneVerificationOtpCommandHandler(
        IOtpService otpService,
        ILogger<RequestPhoneVerificationOtpCommandHandler> logger,
        ITokenService tokenService)
    {
        _otpService = otpService;
        _logger = logger;
        _tokenService = tokenService;
    }

    public async Task<GenericResponseModel<string>> Handle(RequestPhoneVerificationOtpCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing phone verification OTP request for {PhoneNumber}", request.PhoneNumber);
        
        try
        {
            var otp = await _otpService.GenerateOtpAsync(request.PhoneNumber, OtpPurpose.PhoneVerification);
            _logger.LogInformation("Generated and sent OTP for phone number {PhoneNumber}", request.PhoneNumber);

            string token = _tokenService.GenerateToken(request.PhoneNumber, TokenPurpose.PhoneVerification);
            _logger.LogInformation("Generated OTP request token for phone number {PhoneNumber}", request.PhoneNumber);
            
            return GenericResponseModel<string>.Success(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating or sending OTP for phone number {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<string>.Failure(
                Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.PhoneNumber), "Failed to send verification code. Please try again.")
                });
        }
    }
}