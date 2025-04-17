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

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(
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
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = user.Id,
            Expires = GetRefreshTokenExpirationDays()
        };
        return refreshToken;
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
    

    public async Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(RefreshToken refreshToken)
    {
        // Generate new tokens
        var user = refreshToken.User;
        var newAccessToken = await GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        refreshToken.Token = newRefreshToken;
        refreshToken.Expires = GetRefreshTokenExpirationDays(); 
        
        await _unitOfWork.Repository<RefreshToken>().Update(refreshToken);
        await _unitOfWork.SaveChanges();
        
        return (newAccessToken, newRefreshToken);
    }


    
    
    public DateTime GetRefreshTokenExpirationDays()
    {
        int daysToExpire = 7;
        /*if (int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out int configDays))
        {
            daysToExpire = configDays;
        }*/
        return DateTime.UtcNow.AddDays(daysToExpire);
    }

    public async Task<string> GeneratePasswordResetToken(User user) => await _userManager.GeneratePasswordResetTokenAsync(user);
}