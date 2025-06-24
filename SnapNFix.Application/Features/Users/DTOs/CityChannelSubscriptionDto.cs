namespace SnapNFix.Application.Features.Users.DTOs;

public class CityChannelSubscriptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int ActiveIssuesCount { get; set; }
    public bool IsSubscribed { get; set; }
}