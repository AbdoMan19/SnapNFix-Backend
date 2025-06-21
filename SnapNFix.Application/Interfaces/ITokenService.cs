using System.Security.Claims;
using RTools_NTS.Util;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Common.Interfaces;

public interface ITokenService : IScoped
{
    // Access Token
    Task<string> GenerateJwtToken(User user, UserDevice device);
    DateTime GetTokenExpiration();

    // Refresh Token
    string GenerateRefreshToken();
    RefreshToken GenerateRefreshToken(UserDevice userDevice);
    Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(RefreshToken refreshToken);
    DateTime GetRefreshTokenExpirationDays();
    Task<bool> RevokeDeviceTokensAsync(Guid userId, string deviceId);
    
    string GenerateToken(string emailOrPhoneNumber , TokenPurpose purpose);

}


