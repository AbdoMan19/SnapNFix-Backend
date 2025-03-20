namespace SnapNFix.Api.DTOs;

public class RegistrationDto
{
    public required string PhoneNumber { get; set; }
    public string? UserName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
}

