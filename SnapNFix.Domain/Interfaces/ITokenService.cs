using System.Security.Claims;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface ITokenService
{
    Task<string> GenerateJwtToken(User user);
    string GenerateRefreshToken();
    DateTime GetTokenExpiration();
    
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken, string ipAddress);
    Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
    Task<RefreshToken> SaveRefreshTokenAsync(User user, string refreshToken, string ipAddress);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
}