using SnapNFix.Domain.Entities;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
namespace SnapNFix.Application.Common.Interfaces;

public interface IDeviceManager : IScoped
{
    Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceId, string deviceName, string platform, string deviceType , string fcmToken);
    Task<UserDevice?> GetDeviceAsync(Guid userId, string deviceId);
    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId);
    Task<bool> DeactivateDeviceAsync(Guid userId, Guid deviceId);
    Task<int> GetActiveDeviceCountAsync(Guid userId);
    public Task<List<string>> GetActiveDevicesFCMAsync(Guid userId);
    public Guid GetCurrentDeviceId();
    public Task<UserDevice?> GetDeviceByIdAsync(Guid userId, Guid userDeviceId);
}


