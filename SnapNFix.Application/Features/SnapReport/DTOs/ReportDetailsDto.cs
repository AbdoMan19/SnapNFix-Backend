using NetTopologySuite.Geometries;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.SnapReport.DTOs;

public class ReportDetailsDto
{
    public Guid Id { get; set; }
    
    public Guid? IssueId { get; set; }
    
    public string? Comment { get; set; }

    public string ImagePath { get; set; } = string.Empty;
    
    //location
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public ImageStatus Status { get; set; } = ImageStatus.Pending;
    
    //dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public ReportCategory Category { get; set; }

}