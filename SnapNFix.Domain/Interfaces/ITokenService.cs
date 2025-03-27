using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface ITokenService
{
    Task<string> GenerateJwtToken(User user);
    string GenerateRefreshToken();
    DateTime GetTokenExpiration();
}