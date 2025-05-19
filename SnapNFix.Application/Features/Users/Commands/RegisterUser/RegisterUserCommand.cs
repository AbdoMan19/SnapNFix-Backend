using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<GenericResponseModel<LoginResponse>>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    /*public string PhoneNumber { get; set; }*/
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string DeviceId { get; set; }
    public string DeviceName { get; set; }
    public string DeviceType { get; set; }
    public string Platform { get; set; }

}
