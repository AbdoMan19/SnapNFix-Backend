using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class RequestForgetPasswordOtpCommandHandler(UserManager<User> userManager , ILogger<RequestForgetPasswordOtpCommandHandler> logger , IUserService userService , IOtpService otpService , IUserValidationService userValidationService) : IRequestHandler<RequestForgetPasswordOtpCommand, GenericResponseModel<bool>>
{
    public async Task<GenericResponseModel<bool>> Handle(RequestForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var (user, error) = await userValidationService.ValidateUserAsync<bool>(request.EmailOrPhoneNumber);
        if (error != null)
        {
            return error;
        }
        await otpService.GenerateOtpAsync(request.EmailOrPhoneNumber , OtpPurpose.ForgotPassword);
        //send otp to email or phone
        return GenericResponseModel<bool>.Success(true);

    }
}