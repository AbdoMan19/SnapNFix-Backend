using NetTopologySuite.Geometries;
using SnapNFix.Domain.Enums;
namespace SnapNFix.Domain.Entities;

public class SnapReport
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    
    public Guid IssueId { get; set; }
    public virtual Issue Issue { get; set; }
    
    public string? Comment { get; set; }
    
    public string ImagePath { get; set; }
    
    //location
    public Point Location { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    
    //dates
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }



    public ReportCategory Category { get; set; }
    
}