using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

public class DeviceManager : IDeviceManager
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeviceManager(IUnitOfWork unitOfWork, IConfiguration configuration , IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceId, string deviceName, string platform, string deviceType)
    {
        var deviceWithSameId = await GetDeviceAsync(userId, deviceId);

        if (deviceWithSameId != null)
        {

            if (deviceWithSameId.RefreshToken != null)
            {
                deviceWithSameId.RefreshToken.Expires = DateTime.UtcNow;
                await _unitOfWork.Repository<RefreshToken>().Update(deviceWithSameId.RefreshToken);
            }

            deviceWithSameId.UserId = userId;
            deviceWithSameId.LastUsedAt = DateTime.UtcNow;
            deviceWithSameId.Platform = platform;
            deviceWithSameId.DeviceType = deviceType;

            await _unitOfWork.Repository<UserDevice>().Update(deviceWithSameId);
            return deviceWithSameId;
        }

        var newDevice = new UserDevice
        {
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            Platform = platform,
            DeviceType = deviceType,
            LastUsedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<UserDevice>().Add(newDevice);

        return newDevice;
    }
    public async Task<UserDevice?> GetDeviceAsync(Guid userId, string deviceId)
    {
        return await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId && d.DeviceId == deviceId)
            .Include(d => d.RefreshToken)
            .FirstOrDefaultAsync();
    }
    public async Task<UserDevice?> GetDeviceByIdAsync(Guid userId, Guid userDeviceId)
    {
        return await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId && d.Id == userDeviceId)
            .Include(d => d.RefreshToken)
            .FirstOrDefaultAsync();
    }


    public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId)
    {
        return await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId)
            .Include(d => d.RefreshToken)
            .OrderByDescending(d => d.LastUsedAt)
            .ToListAsync();
    }

    public async Task<bool> DeactivateDeviceAsync(Guid userId, Guid deviceId)
    {
        var device = await GetDeviceByIdAsync(userId, deviceId);
        
        if (device?.RefreshToken == null) return false;
        device.LastUsedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<UserDevice>().Update(device);

        device.RefreshToken.Expires = DateTime.UtcNow;
        await _unitOfWork.Repository<RefreshToken>().Update(device.RefreshToken);
        
        return true;
    }

    public async Task<int> GetActiveDeviceCountAsync(Guid userId)
    {
        return await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId)
            .CountAsync();
    }
    public Guid GetCurrentDeviceId()
    {
        var currentDeviceId = _httpContextAccessor.HttpContext?.User.FindFirstValue("DeviceId");
        if (!Guid.TryParse(currentDeviceId, out var DeviceId))
        {
            return Guid.Empty;
        }
        return DeviceId;
    }

}