namespace SnapNFix.Domain.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(Guid userId, string email, IList<string> roles);
    string GenerateRefreshToken();
    DateTime GetTokenExpiration();
}