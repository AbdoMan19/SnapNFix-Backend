using SnapNFix.Domain.Entities;

public interface IDeviceManager
{
    Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceId, string deviceName, string platform, string deviceType);
    Task<UserDevice?> GetDeviceAsync(Guid userId, string deviceId);
    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId);
    Task<bool> DeactivateDeviceAsync(Guid userId, string deviceId);
    Task<int> GetActiveDeviceCountAsync(Guid userId);
    public Guid? GetCurrentDeviceId();
}