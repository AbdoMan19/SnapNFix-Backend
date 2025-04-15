using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.TokenService;

public class JwtService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public JwtService(
        IConfiguration configuration,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        };
        
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = GetTokenExpiration();

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public DateTime GetTokenExpiration()
    {
        double hoursToExpire = 1;
        if (double.TryParse(_configuration["Jwt:TokenExpirationHours"], out double configHours))
        {
            hoursToExpire = configHours;
        }
        return DateTime.UtcNow.AddHours(hoursToExpire);
    }
    
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, 
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public async Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken, string ipAddress)
    {
        // Validate the expired access token

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            throw new SecurityTokenException("Invalid access token");
        }

        // Get user ID from claims
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new SecurityTokenException("Invalid user ID in token");
        }
        
        var storedRefreshToken = await GetRefreshTokenAsync(refreshToken);
        if (storedRefreshToken == null || !storedRefreshToken.IsActive)
        {
            throw new SecurityTokenException("Invalid or expired refresh token");
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new SecurityTokenException("User not found");
        }
        
        // Generate new tokens
        var newAccessToken = await GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        
        // Save new refresh token
        await SaveRefreshTokenAsync(user, newRefreshToken, ipAddress);
        
        // Revoke old refresh token
        storedRefreshToken.Revoked = DateTime.UtcNow;
        storedRefreshToken.ReplacedByToken = newRefreshToken;
        storedRefreshToken.ReasonRevoked = "Replaced by new token";
        
        // Update using repository
        await _unitOfWork.Repository<RefreshToken>().Update(storedRefreshToken);
        await _unitOfWork.SaveChanges();
        
        return (newAccessToken, newRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await GetRefreshTokenAsync(refreshToken);
        if (token != null && token.IsActive)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = "Revoked without replacement";
            
            await _unitOfWork.Repository<RefreshToken>().Update(token);
            await _unitOfWork.SaveChanges();
        }
    }

    public async Task<RefreshToken> SaveRefreshTokenAsync(User user, string refreshToken, string ipAddress)
    {
        var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id.ToString(),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            Expires = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays())
        };
        
        var result = _unitOfWork.Repository<RefreshToken>().Add(token);
        await _unitOfWork.SaveChanges();
        
        return result;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _unitOfWork.Repository<RefreshToken>()
            .SingleOrDefault(rt => rt.Token == token);
    }
    
    private int GetRefreshTokenExpirationDays()
    {
        int daysToExpire = 7;
        if (int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out int configDays))
        {
            daysToExpire = configDays;
        }
        return daysToExpire;
    }
}