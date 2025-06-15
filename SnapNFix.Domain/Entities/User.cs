using Microsoft.AspNetCore.Identity;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => FirstName + " " + LastName;
    public string ImagePath { get; set; } = string.Empty;
    public string Username { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
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