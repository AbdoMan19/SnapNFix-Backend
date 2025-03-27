using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.OtpService;

//todo convert it to redis

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _otpLifetime = TimeSpan.FromMinutes(5);

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> GenerateOtpAsync(string phoneNumber)
    {
        string otp = GenerateRandomOtp();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_otpLifetime);

        _cache.Set($"OTP_{phoneNumber}", otp, cacheEntryOptions);
        
        return Task.FromResult(otp);
    }

    public Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
    {
        if (_cache.TryGetValue($"OTP_{phoneNumber}", out string? storedOtp) && storedOtp != null)
        {
            return Task.FromResult(storedOtp == otp);
        }
        
        return Task.FromResult(false);
    }

    public Task InvalidateOtpAsync(string phoneNumber)
    {
        _cache.Remove($"OTP_{phoneNumber}");
        return Task.CompletedTask;
    }

    private static string GenerateRandomOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }
}