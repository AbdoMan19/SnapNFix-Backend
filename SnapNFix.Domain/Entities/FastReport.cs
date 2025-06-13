using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Entities;

public class FastReport
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Comment { get; set; }

    public Severity Severity { get; set; } = Severity.Low;


}