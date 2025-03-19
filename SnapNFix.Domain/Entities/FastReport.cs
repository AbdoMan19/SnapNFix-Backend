namespace SnapNFix.Domain.Entities;

public class FastReport
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string? Comment { get; set; }

    
}