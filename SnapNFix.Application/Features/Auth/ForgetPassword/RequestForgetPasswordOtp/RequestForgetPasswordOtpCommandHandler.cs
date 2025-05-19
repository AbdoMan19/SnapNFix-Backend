using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Common.Services.UserValidationServices;
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
        try
        {
            // Step 1: Validate user
            var (user, error) = await userValidationService.ValidateUserAsync<string>(request.EmailOrPhoneNumber);
            if (error != null)
            {
                return error;
            }

            // Step 2: Generate OTP in memory cache
            await otpService.InvalidateOtpAsync(request.EmailOrPhoneNumber, OtpPurpose.ForgotPassword);
            var otp = await otpService.GenerateOtpAsync(request.EmailOrPhoneNumber, OtpPurpose.ForgotPassword);

            // Step 3: Generate token (may involve database operations)
            var resetRequestToken = tokenService.GenerateToken(request.EmailOrPhoneNumber , TokenPurpose.PasswordResetVerification);
            
            // Step 4: TODO - Send OTP to user

            logger.LogInformation("Generated password reset token for user {UserId}", user.Id);
            return GenericResponseModel<string>.Success(resetRequestToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing forgot password request for {EmailOrPhone}", 
                request.EmailOrPhoneNumber);
            return GenericResponseModel<string>.Failure("An error occurred processing your request");
        }
    }
}