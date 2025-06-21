using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDeviceManager _deviceManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IDeviceManager deviceManager,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger)
    {
        _unitOfWork = unitOfWork;
        _deviceManager = deviceManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> AuthenticateUserAsync(
        User user, 
        string deviceId, 
        string deviceName, 
        string platform, 
        string deviceType, 
        string fcmToken)
    {
        _logger.LogInformation("Authenticating user {UserId} on device {DeviceId}", user.Id, deviceId);
        
        // Register/associate device with current user - this will handle logging out previous users
        var userDevice = await _deviceManager.RegisterDeviceAsync(
            user.Id,
            deviceId,
            deviceName,
            platform,
            deviceType,
            fcmToken ?? string.Empty); // Handle null FCM token

        // Generate JWT token
        var accessToken = await _tokenService.GenerateJwtToken(user, userDevice);
        string refreshTokenString;

        // Handle refresh token creation/update
        if (userDevice.RefreshToken != null)
        {
            await _unitOfWork.Repository<UserDevice>().Update(userDevice);

            refreshTokenString = _tokenService.GenerateRefreshToken();
            userDevice.RefreshToken.Token = refreshTokenString;
            userDevice.RefreshToken.Expires = _tokenService.GetRefreshTokenExpirationDays();
            userDevice.RefreshToken.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<RefreshToken>().Update(userDevice.RefreshToken);
        }
        else
        {
            var newUserDevice = await _unitOfWork.Repository<UserDevice>().Add(userDevice);
            await _unitOfWork.SaveChanges();
            var refreshTokenObj = new RefreshToken
            {
                Token = _tokenService.GenerateRefreshToken(),
                UserDeviceId = newUserDevice.Id,
                Expires = _tokenService.GetRefreshTokenExpirationDays(),
                CreatedAt = DateTime.UtcNow
            };
            refreshTokenString = refreshTokenObj.Token;
            await _unitOfWork.Repository<RefreshToken>().Add(refreshTokenObj);
            userDevice.RefreshToken = refreshTokenObj;
            userDevice.RefreshTokenId = refreshTokenObj.Id;
            await _unitOfWork.Repository<UserDevice>().Update(newUserDevice);
        }
        
        _logger.LogInformation("User {UserId} authenticated successfully on device {DeviceId}", 
            user.Id, deviceId);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = userDevice.RefreshToken.Expires
        };
    }

    public async Task<bool> RevokeAuthenticationAsync(Guid userId, string deviceId)
    {
        _logger.LogInformation("Revoking authentication for user {UserId} on device {DeviceId}", userId, deviceId);
        
        var userDevice = await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId && d.DeviceId == deviceId)
            .Include(d => d.RefreshToken)
            .FirstOrDefaultAsync();

        if (userDevice == null)
        {
            _logger.LogWarning("No device found for user {UserId} with device ID {DeviceId}", userId, deviceId);
            return false;
        }

        if (userDevice.RefreshToken != null)
        {
            // Expire the refresh token to log out
            userDevice.RefreshToken.Expires = DateTime.UtcNow;
            await _unitOfWork.Repository<RefreshToken>().Update(userDevice.RefreshToken);
            await _unitOfWork.SaveChanges();
            
            _logger.LogInformation("Successfully revoked authentication for user {UserId} on device {DeviceId}", 
                userId, deviceId);
            return true;
        }

        _logger.LogWarning("No refresh token found for user {UserId} with device ID {DeviceId}", userId, deviceId);
        return false;
    }
}
