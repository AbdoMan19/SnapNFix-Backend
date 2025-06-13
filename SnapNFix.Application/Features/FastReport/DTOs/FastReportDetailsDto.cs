namespace SnapNFix.Application.Features.FastReport.DTOs;

public class FastReportDetailsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid IssueId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
}