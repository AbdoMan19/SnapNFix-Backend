using SnapNFix.Domain.Enums;

public class UserDetailsDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string>? Roles { get; set; }
}