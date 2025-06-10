using NetTopologySuite.Geometries;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Entities;

public class SnapReport
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    
    public Guid? IssueId { get; set; }
    public virtual Issue? Issue { get; set; }

    public string Comment { get; set; } = string.Empty;

    public string ImagePath { get; set; }
    
    public Point Location { get; set; }
    
    public ImageStatus ImageStatus { get; set; } = ImageStatus.Pending;
    public string? TaskId { get; set; }
    public double? Threshold { get; set; }
    
    public ReportCategory ReportCategory { get; set; } = ReportCategory.NotSpecified;
    
    public Severity Severity { get; set; } = Severity.Medium;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}