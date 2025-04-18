using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class RequestForgetPasswordOtpCommandHandler(UserManager<User> userManager , ILogger<RequestForgetPasswordOtpCommandHandler> logger , IUserService userService , IOtpService otpService) : IRequestHandler<RequestForgetPasswordOtpCommand, GenericResponseModel<bool>>
{
    public async Task<GenericResponseModel<bool>> Handle(RequestForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create(nameof(request.EmailOrPhoneNumber), "Invalid Email or Phone Number")
        };
        (var isEmail , var isPhone , var user) = await userService.IsEmailOrPhone(request.EmailOrPhoneNumber);
        if (user == null)
        {
            logger.LogWarning("Login attempt failed: User not found for identifier {Identifier}", nameof(request.EmailOrPhoneNumber));
            return GenericResponseModel<bool>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        await otpService.GenerateOtpAsync(request.EmailOrPhoneNumber , OtpPurpose.ForgotPassword);
        if (isEmail)
        {
            
        }
        else if(isPhone)
        {
            
        }
        return GenericResponseModel<bool>.Success(true);

    }
}