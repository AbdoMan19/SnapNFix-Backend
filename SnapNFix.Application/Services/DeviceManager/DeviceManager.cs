using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapNFix.Domain.Entities;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Interfaces;

public class DeviceManager : IDeviceManager
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DeviceManager> _logger;

    public DeviceManager(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<DeviceManager> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }


    public async Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceId, string deviceName, string platform, string deviceType, string fcm)
    {
        var deviceWithSameId = await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.DeviceId == deviceId)
            .Include(d => d.RefreshToken)
            .FirstOrDefaultAsync();

        if (deviceWithSameId != null)
        {
            // If device exists but with different user, log out previous user
            if (deviceWithSameId.UserId != userId)
            {
                _logger.LogInformation("Device {DeviceId} is already registered to user {PreviousUserId}. " +
                                       "Logging out previous user and associating with user {NewUserId}",
                    deviceId, deviceWithSameId.UserId, userId);
                    
                // Expire existing refresh token to log out previous user
                if (deviceWithSameId.RefreshToken != null && !deviceWithSameId.RefreshToken.IsExpired)
                {
                    _logger.LogInformation("Expiring refresh token for device {DeviceId} of user {PreviousUserId}",
                        deviceId, deviceWithSameId.UserId);
                   
                    deviceWithSameId.RefreshToken.Expires = DateTime.UtcNow;
                    await _unitOfWork.Repository<RefreshToken>().Update(deviceWithSameId.RefreshToken);
                }
            }

            // Update the device with new user and information
            deviceWithSameId.UserId = userId;
            deviceWithSameId.LastUsedAt = DateTime.UtcNow;
            deviceWithSameId.Platform = platform;
            deviceWithSameId.DeviceType = deviceType;
            deviceWithSameId.DeviceName = deviceName;
            deviceWithSameId.FCMToken = fcm;
            

            return deviceWithSameId;
        }

        _logger.LogInformation("Registering new device {DeviceId} for user {UserId}", deviceId, userId);
        
        var newDevice = new UserDevice
        {
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            Platform = platform,
            DeviceType = deviceType,
            LastUsedAt = DateTime.UtcNow,
            FCMToken = fcm // This will be empty string if not provided
        };
        
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
        
        _unitOfWork.Repository<RefreshToken>().Delete(device.RefreshToken.Id);
        
        _unitOfWork.Repository<UserDevice>().Delete(device.Id);
        
        return true;
    }

    public async Task<int> GetActiveDeviceCountAsync(Guid userId)
    {
        return await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId && d.RefreshToken != null && !d.RefreshToken.IsExpired)
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
    //get fcm of active device
    public async Task<List<string>> GetActiveDevicesFCMAsync(Guid userId)
    {
        var tokens = await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId )
            .Where(d => !string.IsNullOrEmpty(d.FCMToken) && d.RefreshToken != null && !d.RefreshToken.IsExpired)
            .Select(d =>  d.FCMToken )
            .ToListAsync();

        return tokens;
    }

}
