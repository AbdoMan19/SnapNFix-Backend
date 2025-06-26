using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.CityChannel.DTOs;

public class CityIssueDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; }
    public ReportCategory Category { get; set; }
    public Severity Severity { get; set; }
    public IssueStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}