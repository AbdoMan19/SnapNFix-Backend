using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Interfaces.ServiceLifetime;
namespace SnapNFix.Domain.Interfaces;

public interface IDeviceManager : IScoped
{
    Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceId, string deviceName, string platform, string deviceType);
    Task<UserDevice?> GetDeviceAsync(Guid userId, string deviceId);
    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId);
    Task<bool> DeactivateDeviceAsync(Guid userId, string deviceId);
    Task<int> GetActiveDeviceCountAsync(Guid userId);
    public Guid? GetCurrentDeviceId();
}