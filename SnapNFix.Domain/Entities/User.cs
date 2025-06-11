using Microsoft.AspNetCore.Identity;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Entities;

public class User : IdentityUser<Guid>
{
    //Name
    public string FirstName { get; set; }
    public string LastName { get; set; }
    //ImagePath
    public string ImagePath { get; set; } = string.Empty;
    //Username
    public string Username { get; set; } = Guid.NewGuid().ToString();
    //Contact
    public string Email { get; set; } = string.Empty;
    // Birthdate
    public DateOnly? BirthDate { get; set; }
    // Gender
    public Gender Gender { get; set; } = Gender.NotSpecified;

    public bool IsSuspended => AccessFailedCount >= 3;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<SnapReport> SnapReports { get; set; } = [];
    public virtual ICollection<FastReport> FastReports { get; set; } = [];
    public virtual ICollection<UserDevice> UserDevices { get; set; } = [];
}