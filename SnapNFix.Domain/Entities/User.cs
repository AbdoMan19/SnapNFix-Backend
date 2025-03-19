using Microsoft.AspNetCore.Identity;

namespace SnapNFix.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ImagePath { get; set; }
    public bool IsSuspended => AccessFailedCount >= 3;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<SnapReport> SnapReports { get; set; } = [];
    public virtual ICollection<FastReport> FastReports { get; set; } = [];

}