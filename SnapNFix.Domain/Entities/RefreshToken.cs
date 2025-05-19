namespace SnapNFix.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public Guid UserDeviceId { get; set; }
    public UserDevice UserDevice { get; set; }
}