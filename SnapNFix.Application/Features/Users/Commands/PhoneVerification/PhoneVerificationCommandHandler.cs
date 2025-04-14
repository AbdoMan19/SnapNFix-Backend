using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.PhoneVerification;

public class PhoneVerificationCommandHandler : IRequestHandler<PhoneVerificationCommand, GenericResponseModel<AuthResponse>>
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

    public async Task<GenericResponseModel<AuthResponse>> Handle(PhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing phone verification for {PhoneNumber}", request.PhoneNumber);
        
        var user = await _unitOfWork.Repository<User>()
            .FindBy(u => u.PhoneNumber == request.PhoneNumber)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (user is null)
        {
            _logger.LogWarning("Phone verification failed: User not found for {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<AuthResponse>.Failure("User not found");
        }

        var valid = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.Otp);
        
        if (valid)
        {
            _logger.LogInformation("OTP verification successful for user {UserId}", user.Id);

            if (!user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
                    _logger.LogWarning("Failed to update verification status for user {UserId} with {ErrorCount} errors", user.Id, errors.Count);
                    return GenericResponseModel<AuthResponse>.Failure("Failed to update verification status", errors);
                }
            }

            var token = await _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _otpService.InvalidateOtpAsync(request.PhoneNumber);

            return GenericResponseModel<AuthResponse>.Success(new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetTokenExpiration()
            });
        }

        _logger.LogWarning("Phone verification failed: Invalid OTP for {PhoneNumber}", request.PhoneNumber);
        return GenericResponseModel<AuthResponse>.Failure("Invalid verification code");
    }
}