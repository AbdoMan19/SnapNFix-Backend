namespace SnapNFix.Application.Features.Auth.Dtos;

public class LoginResponse
{
    public AuthResponse Tokens { get; set; }
    public UserInfo User { get; set; }

    public class UserInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string ImagePath { get; set; } = String.Empty;
    }
}