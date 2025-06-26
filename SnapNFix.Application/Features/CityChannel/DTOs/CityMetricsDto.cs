using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.CityChannel.DTOs;

public class CityMetricsDto
{
    public int HealthPercentage { get; set; }
    public HealthCondition HealthCondition { get; set; }
    public int PendingIssuesCount { get; set; }
    public int InProgressIssuesCount { get; set; }
    public int FixedIssuesCount { get; set; }
    public int TotalSubscribers { get; set; }
}