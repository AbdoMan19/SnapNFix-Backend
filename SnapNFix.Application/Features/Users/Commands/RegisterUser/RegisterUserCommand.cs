using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<GenericResponseModel<Guid>>
{
    public  string FirstName { get; set; }
    public  string LastName { get; set; }
    public  string PhoneNumber { get; set; }
    public  string Password { get; set; }
    public  string ConfirmPassword { get; set; }
    public  string Email { get; set; } 

}
