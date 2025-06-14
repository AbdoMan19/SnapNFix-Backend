using Microsoft.AspNetCore.Identity;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Common.Interfaces;

public interface IAuthenticationService : IScoped
{
    /// <summary>
    /// Handles device registration and authentication for a user
    /// </summary>
    /// <param name="user">The user to authenticate</param>
    /// <param name="deviceId">Unique device identifier</param>
    /// <param name="deviceName">Name of the device</param>
    /// <param name="platform">Platform (e.g., "web", "mobile", "ios", "android")</param>
    /// <param name="deviceType">Type of device</param>
    /// <param name="fcmToken">Firebase Cloud Messaging token (optional)</param>
    /// <returns>Authentication response with tokens</returns>
    Task<AuthResponse> AuthenticateUserAsync(
        User user, 
        string deviceId, 
        string deviceName, 
        string platform, 
        string deviceType, 
        string fcmToken);
    
    /// <summary>
    /// Revokes authentication for a user on a specific device
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Device ID</param>
    /// <returns>True if successfully revoked</returns>
    Task<bool> RevokeAuthenticationAsync(Guid userId, string deviceId);
}