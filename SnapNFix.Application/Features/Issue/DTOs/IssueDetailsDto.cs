using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Issue.DTOs;

public class IssueDetailsDto
{
    public Guid Id { get; set; }
    public ReportCategory Category { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }

    public IssueStatus Status { get; set; }
    public Severity Severity { get; set; }
    public List<string> Images { get; set; } = new List<string>();

    public string Road { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public int ReportsCount { get; set; }

}