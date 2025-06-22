using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Resources;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using System.Security.Claims;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;

public class PhoneVerificationCommandHandler : IRequestHandler<PhoneVerificationCommand, GenericResponseModel<string>>
{
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly ILogger<PhoneVerificationCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PhoneVerificationCommandHandler(
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IOtpService otpService,
        ILogger<PhoneVerificationCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _otpService = otpService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponseModel<string>> Handle(PhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var contactClaim = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == "contact")?.Value;
            
        _logger.LogInformation("Processing phone verification for {PhoneNumber}", contactClaim);
        var isOtpValid = await _otpService.VerifyOtpAsync(contactClaim, request.Otp, OtpPurpose.PhoneVerification);
        if (!isOtpValid)
        {
            _logger.LogWarning("Phone verification failed: Invalid OTP for {PhoneNumber}", contactClaim);
            return GenericResponseModel<string>.Failure(Constants.FailureMessage , new List<ErrorResponseModel>{ErrorResponseModel.Create(
                nameof(request.Otp) , Shared.InvalidOtp )});
        }
        
        _logger.LogInformation("Phone verification successful for {PhoneNumber}", contactClaim);
        var token = _tokenService.GenerateToken(contactClaim, TokenPurpose.Registration);
        _logger.LogInformation("Generated Registration token for user {PhoneNumber}", contactClaim);
        
        return GenericResponseModel<string>.Success(token);
    }
}

