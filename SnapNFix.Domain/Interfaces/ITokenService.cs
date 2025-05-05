using System.Security.Claims;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

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

    // Password Reset
    Task<string> GeneratePasswordResetToken(User user);
    Task<string> GeneratePasswordResetRequestTokenAsync(User user);
    Task<bool> ValidatePasswordResetRequestTokenAsync(User user, string token);
    bool ValidatePasswordResetTokenAsync(User user, string token);

    // Phone number verification
    
    Task<string> GenerateOtpRequestToken(string phoneNumber);
    Task<string> GenerateRegistrationToken(string phoneNumber);

}