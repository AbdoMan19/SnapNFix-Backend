using NetTopologySuite.Geometries;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Entities;

public class Issue
{
    public Guid Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;
    public ReportCategory Category { get; set; }
    public Point Location { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public IssueStatus Status { get; set; } = IssueStatus.Pending;
    
    public Severity Severity { get; set; } = Severity.Unspecified;

    public virtual ICollection<SnapReport> AssociatedSnapReports { get; set; } = [];
    public virtual ICollection<FastReport> AssociatedFastReports { get; set; } = [];
}