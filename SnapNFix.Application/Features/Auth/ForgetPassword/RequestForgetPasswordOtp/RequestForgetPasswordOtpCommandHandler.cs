using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class RequestForgetPasswordOtpCommandHandler(
    UserManager<User> userManager, 
    ILogger<RequestForgetPasswordOtpCommandHandler> logger, 
    IUserService userService, 
    IOtpService otpService, 
    ITokenService tokenService,
    IUserValidationService userValidationService) : IRequestHandler<RequestForgetPasswordOtpCommand, GenericResponseModel<string>>
{
    public async Task<GenericResponseModel<string>> Handle(RequestForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var (user, error) = await userValidationService.ValidateUserAsync<string>(request.EmailOrPhoneNumber);

        if (error != null )
        {
            return error;
        }

        
        var otp = await otpService.GenerateOtpAsync(request.EmailOrPhoneNumber, OtpPurpose.ForgotPassword);

        // TODO : Send OTP to user via SMS
        
        var resetRequestToken = await tokenService.GeneratePasswordResetRequestTokenAsync(user);
        
        logger.LogInformation("Generated password reset request token for user {UserId}", user.Id);
        
        return GenericResponseModel<string>.Success(resetRequestToken);
    }
}