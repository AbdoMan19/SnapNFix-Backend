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
    
    //location
    public Point Location { get; set; }
    
    //ai response
    public ImageStatus ImageStatus { get; set; } = ImageStatus.Pending;
    public string? TaskId { get; set; }
    public double? Threshold { get; set; }
    
    //report category
    public ReportCategory ReportCategory { get; set; } = ReportCategory.NotSpecified;
    
    //dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    
}