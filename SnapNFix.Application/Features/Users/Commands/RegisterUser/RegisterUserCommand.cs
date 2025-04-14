using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<GenericResponseModel<Guid>>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public string? Email { get; set; } = null;

}
