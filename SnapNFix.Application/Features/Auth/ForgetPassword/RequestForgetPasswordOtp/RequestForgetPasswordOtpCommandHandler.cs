using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Common.Services.UserValidationServices;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class RequestForgetPasswordOtpCommandHandler : IRequestHandler<RequestForgetPasswordOtpCommand, GenericResponseModel<string>>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RequestForgetPasswordOtpCommandHandler> _logger;
    private readonly IUserService _userService;
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;
    private readonly IUserValidationService _userValidationService;

    public RequestForgetPasswordOtpCommandHandler(
        UserManager<User> userManager, 
        ILogger<RequestForgetPasswordOtpCommandHandler> logger, 
        IUserService userService, 
        IOtpService otpService, 
        ITokenService tokenService,
        IUserValidationService userValidationService)
    {
        _userManager = userManager;
        _logger = logger;
        _userService = userService;
        _otpService = otpService;
        _tokenService = tokenService;
        _userValidationService = userValidationService;
    }

    public async Task<GenericResponseModel<string>> Handle(RequestForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (user, error) = await _userValidationService.ValidateUserAsync<string>(request.EmailOrPhoneNumber);
            if (error != null)
            {
                return error;
            }

            await _otpService.InvalidateOtpAsync(request.EmailOrPhoneNumber, OtpPurpose.ForgotPassword);
            var otp = await _otpService.GenerateOtpAsync(request.EmailOrPhoneNumber, OtpPurpose.ForgotPassword);

            var resetRequestToken = _tokenService.GenerateToken(request.EmailOrPhoneNumber, TokenPurpose.PasswordResetVerification);
            
            _logger.LogInformation("Generated password reset token for user {UserId}", user.Id);
            return GenericResponseModel<string>.Success(resetRequestToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for {EmailOrPhone}", 
                request.EmailOrPhoneNumber);
            return GenericResponseModel<string>.Failure(Shared.UnexpectedError);
        }
    }
}