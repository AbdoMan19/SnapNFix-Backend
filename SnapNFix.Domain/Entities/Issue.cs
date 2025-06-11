using NetTopologySuite.Geometries;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Entities;

public class Issue
{
    public Guid Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;
    public ReportCategory Category { get; set; }
    public Point Location { get; set; }
    
    // Address properties
    public string Road { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = "Egypt";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public IssueStatus Status { get; set; } = IssueStatus.Pending;
    public Severity Severity { get; set; } = Severity.Unspecified;

    public virtual ICollection<SnapReport> AssociatedSnapReports { get; set; } = [];
    public virtual ICollection<FastReport> AssociatedFastReports { get; set; } = [];
    
    // Computed property for full address
    public string FullAddress => 
        string.Join(", ", new[] { Road, City, State, Country }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
}