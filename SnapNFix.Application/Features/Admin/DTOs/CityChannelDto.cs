namespace SnapNFix.Application.Features.Admin.DTOs;

public class CityChannelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SubscriberCount { get; set; }
    public int IssueCount { get; set; }
}