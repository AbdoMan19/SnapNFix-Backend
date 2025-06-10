using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Issue.DTOs;

public class IssueDetailsDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}