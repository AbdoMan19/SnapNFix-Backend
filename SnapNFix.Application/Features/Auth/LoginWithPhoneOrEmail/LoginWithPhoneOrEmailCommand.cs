using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommand : IRequest<GenericResponseModel<LoginResponse>>
{
    public string EmailOrPhoneNumber { get; set; }
    public string Password { get; set; }
}