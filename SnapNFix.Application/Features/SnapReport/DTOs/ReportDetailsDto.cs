using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.SnapReport.DTOs;

public class ReportDetailsDto
{
    public Guid Id { get; set; }
    
    public Guid? IssueId { get; set; }
    
    public string? Comment { get; set; }

    public string ImagePath { get; set; } = string.Empty;
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    //address details
    public string Road { get; set; } 
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }

    
    public ImageStatus Status { get; set; } = ImageStatus.Pending;
    
    public ReportCategory Category { get; set; }
    
    public Severity Severity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}