namespace SnapNFix.Domain.Entities;

public class UserDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string DeviceName { get; set; }
    public string DeviceId { get; set; }
    public string Platform { get; set; }
    public string DeviceType { get; set; }
    
    public RefreshToken? RefreshToken { get; set; }
    public Guid? RefreshTokenId { get; set; } 
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
}