
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandHandler(IUnitOfWork unitOfWork , UserManager<User> userManager , ITokenService tokenService) : IRequestHandler<LoginWithPhoneOrEmailCommand , GenericResponseModel<string>>
{
    public async Task<GenericResponseModel<string>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Repository<User>().FindBy(u => u.Email == request.PhoneOrEmail || u.PhoneNumber == request.PhoneOrEmail).FirstOrDefaultAsync();
        if (user == null)
        {
            return GenericResponseModel<string>
                .Failure(Constants.FailureMessage 
                    , new List<ErrorResponseModel>{ErrorResponseModel
                        .Create(nameof(request.PhoneOrEmail) , "User not found")});
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return GenericResponseModel<string>
                .Failure(Constants.FailureMessage 
                    , new List<ErrorResponseModel>{ErrorResponseModel
                        .Create(nameof(request.Password) , "Invalid password")});
        }

        var token = await tokenService.GenerateJwtToken(user);
        return GenericResponseModel<string>.Success(token);
    }
}