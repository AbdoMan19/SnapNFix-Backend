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

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    //address details
    public string Road { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    
    public ImageStatus Status { get; set; } = ImageStatus.Pending;
    
    public ReportCategory Category { get; set; }
    
    public Severity Severity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}