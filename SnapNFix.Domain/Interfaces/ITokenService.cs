using System.Security.Claims;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface ITokenService
{
    //AccessToken
    Task<string> GenerateJwtToken(User user);
    DateTime GetTokenExpiration();
    
    
    //RefreshToken
    string GenerateRefreshToken();
    RefreshToken GenerateRefreshToken(User user);
    Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(RefreshToken refresh);
    public DateTime GetRefreshTokenExpirationDays();
}