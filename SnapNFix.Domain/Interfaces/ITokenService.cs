using System.Security.Claims;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface ITokenService
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

    // Password Reset
    Task<string> GeneratePasswordResetToken(User user);

    // Phone number verification
    
    Task<string> GeneratePhoneVerificationTokenAsync(User user);
    Task<bool> ValidatePhoneVerificationTokenAsync(string phoneNumber, string token);
}