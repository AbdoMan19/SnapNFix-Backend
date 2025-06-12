using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Issue.DTOs;

public class IssueDetailsDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();

    public string Road { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public int ReportsCount { get; set; }
}