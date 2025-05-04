using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IDeviceManager _deviceManager;

    public TokenService(
        IConfiguration configuration,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IDeviceManager deviceManager)
    {
        _configuration = configuration;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _deviceManager = deviceManager;
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensForDeviceAsync(
        User user,
        string deviceId,
        string deviceName,
        string platform,
        string deviceType)
    {
        var userDevice = await _deviceManager.RegisterDeviceAsync(
            user.Id,
            deviceId,
            deviceName,
            platform,
            deviceType);

        var accessToken = await GenerateJwtToken(user, userDevice);
        var refreshToken = GenerateRefreshToken(userDevice);

        userDevice.RefreshToken = refreshToken;
        await _unitOfWork.Repository<UserDevice>().Update(userDevice);
        await _unitOfWork.SaveChanges();

        return (accessToken, refreshToken.Token);
    }

    public async Task<string> GenerateJwtToken(User user, UserDevice device)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(new[]
        {
            new Claim("DeviceId", device.DeviceId),
            new Claim("DeviceName", device.DeviceName),
            new Claim("Platform", device.Platform)
        });

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

    public RefreshToken GenerateRefreshToken(UserDevice userDevice)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserDeviceId = userDevice.Id,
            Expires = GetRefreshTokenExpirationDays(),
            Created = DateTime.UtcNow
        };
        return refreshToken;
    }

    public DateTime GetTokenExpiration()
    {
        double minutesToExpire = 5;
        if (double.TryParse(_configuration["Jwt:TokenExpirationMinutes"], out double configMinutes))
        {
            minutesToExpire = configMinutes;
        }
        return DateTime.UtcNow.AddHours(minutesToExpire);
    }


    public async Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(RefreshToken refreshToken)
    {
        if (refreshToken.IsExpired || refreshToken.IsRevoked)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        var userDevice = await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.Id == refreshToken.UserDeviceId)
            .Include(d => d.User)
            .FirstOrDefaultAsync();

        var user = userDevice?.User;
        var newAccessToken = await GenerateJwtToken(user, userDevice);
        var newRefreshToken = GenerateRefreshToken(userDevice);

        refreshToken.Token = newRefreshToken.Token;
        refreshToken.Expires = GetRefreshTokenExpirationDays();

        userDevice.LastUsedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<UserDevice>().Update(userDevice);
        await _unitOfWork.Repository<RefreshToken>().Update(refreshToken);
        await _unitOfWork.SaveChanges();

        return (newAccessToken, newRefreshToken.Token);
    }


    public async Task<bool> RevokeDeviceTokensAsync(Guid userId, string deviceId)
    {
        var userDevice = await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == userId && d.DeviceId == deviceId)
            .Include(d => d.RefreshToken)
            .FirstOrDefaultAsync();

        if (userDevice?.RefreshToken == null)
        {
            return false;
        }

        userDevice.RefreshToken.Revoked = DateTime.UtcNow;
        await _unitOfWork.Repository<RefreshToken>().Update(userDevice.RefreshToken);
        await _unitOfWork.SaveChanges();

        return true;
    }

    public DateTime GetRefreshTokenExpirationDays()
    {
        int daysToExpire = 7;
        if (int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out int configDays))
        {
            daysToExpire = configDays;
        }
        return DateTime.UtcNow.AddDays(daysToExpire);
    }


    public async Task<string> GenerateOtpRequestToken(string phoneNumber)
    {
        var claims = new List<Claim>
        {
            new("phone", phoneNumber),
            new("purpose", "otp_request"),
            new("issued_at", DateTime.UtcNow.ToString("O"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(5); // Short-lived token

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<string> GenerateRegistrationToken(string phoneNumber)
    {
        var claims = new List<Claim>
        {
            new("phone", phoneNumber),
            new("purpose", "registration"),
            new("issued_at", DateTime.UtcNow.ToString("O"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1); // Give them some time to register

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public async Task<string> GeneratePasswordResetRequestTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new("purpose", "password_reset_request")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(10);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> ValidatePasswordResetRequestTokenAsync(User user, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var purposeClaim = principal.FindFirst(c => c.Type == "purpose");
            if (purposeClaim == null || purposeClaim.Value != "password_reset_request")
                return false;

            var userIdClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || userIdClaim.Value != user.Id.ToString())
                return false;

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<string> GeneratePasswordResetToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("email", user.Email ?? string.Empty),
            new("purpose", "password_reset")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(10);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidatePasswordResetTokenAsync(User user, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var purposeClaim = principal.FindFirst(c => c.Type == "purpose");
            if (purposeClaim == null || purposeClaim.Value != "password_reset")
                return false;

            var userIdClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || userIdClaim.Value != user.Id.ToString())
                return false;

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }



}