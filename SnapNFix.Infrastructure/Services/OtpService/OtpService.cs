using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using SnapNFix.Domain.Enums;
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

    public Task<string> GenerateOtpAsync(string emailOrPhoneNumber, OtpPurpose otpPurpose)
    {
        string otp = GenerateRandomOtp();
        var cacheKey = GenerateCacheKey(emailOrPhoneNumber, otpPurpose);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_otpLifetime);

        _cache.Set(cacheKey, otp, cacheEntryOptions);
        
        return Task.FromResult(otp);
    }

    public Task<bool> VerifyOtpAsync(string emailOrPhoneNumber, string otp, OtpPurpose purpose)
    {
        var cacheKey = GenerateCacheKey(emailOrPhoneNumber, purpose);
        if (_cache.TryGetValue(cacheKey, out string? storedOtp) && storedOtp != null)
        {
            return Task.FromResult(storedOtp == otp);
        }
        
        return Task.FromResult(false);
    }

    public Task InvalidateOtpAsync(string emailOrPhoneNumber, OtpPurpose purpose)
    {
        var cacheKey = GenerateCacheKey(emailOrPhoneNumber, purpose);

        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    private static string GenerateRandomOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }
    private static string GenerateCacheKey(string phoneOrEmail, OtpPurpose purpose)
    {
        return $"OTP:{purpose}:{phoneOrEmail}";
    }
}