using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class ForgetPasswordCommandHandler(UserManager<User> userManager , ILogger<ForgetPasswordCommandHandler> logger) : IRequestHandler<RequestForgetPasswordOtpCommand, GenericResponseModel<bool>>
{
    public async Task<GenericResponseModel<bool>> Handle(RequestForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create("Authentication", "Invalid credentials")
        };
        User user = null;
        var isEmail = false;
        var isPhone = false;

        if (request.EmailOrPhoneNumber.Contains("@"))
        {
            isEmail = true;
            user = await userManager.FindByEmailAsync(request.EmailOrPhoneNumber);
        }
        else
        {
            isPhone = true;
            user = await userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.EmailOrPhoneNumber, cancellationToken);
        }
        if (user == null)
        {
            logger.LogWarning("Login attempt failed: User not found for identifier {Identifier}", nameof(request.EmailOrPhoneNumber));
            return GenericResponseModel<bool>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        if (isEmail)
        {
            
        }
        else
        {
            
        }
        return GenericResponseModel<bool>.Success(true);

    }
}