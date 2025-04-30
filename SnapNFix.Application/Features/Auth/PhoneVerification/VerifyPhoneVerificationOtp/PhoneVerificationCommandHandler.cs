using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;

public class PhoneVerificationCommandHandler : IRequestHandler<PhoneVerificationCommand, GenericResponseModel<bool>>
{
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly ILogger<PhoneVerificationCommandHandler> _logger;

    public PhoneVerificationCommandHandler(
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IOtpService otpService,
        ILogger<PhoneVerificationCommandHandler> logger)
    {
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<bool>> Handle(PhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing phone verification for {PhoneNumber}", request.PhoneNumber);

        var user = await _unitOfWork.Repository<User>()
            .FindBy(u => u.PhoneNumber == request.PhoneNumber)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("Phone verification failed: User not found for {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<bool>.Failure("User not found");
        }

        if (user.PhoneNumberConfirmed)
        {
            _logger.LogInformation("Phone number {PhoneNumber} is already verified", request.PhoneNumber);
            return GenericResponseModel<bool>.Failure("Phone number is already verified");
        }
        
        var tokenValid = await _tokenService.ValidatePhoneVerificationTokenAsync(request.PhoneNumber, request.VerificationToken);
        if (!tokenValid)
        {
            _logger.LogWarning("Phone verification failed: Invalid verification token for {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<bool>.Failure("Invalid verification token");
        }
        
        var isOtpValid = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.Otp, OtpPurpose.PhoneVerification);
        if (!isOtpValid)
        {
            _logger.LogWarning("Phone verification failed: Invalid OTP for {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<bool>.Failure("Invalid OTP");
        }
        
        
        
        user.PhoneNumberConfirmed = true;
        await _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChanges();
        
        _logger.LogInformation("Phone verification successful for {PhoneNumber}", request.PhoneNumber);
        return GenericResponseModel<bool>.Success(true);
    }
}