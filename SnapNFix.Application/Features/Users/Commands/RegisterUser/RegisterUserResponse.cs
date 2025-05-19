namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserResponse
{
    public Guid UserId { get; set; }
    public string VerificationToken { get; set; }
}