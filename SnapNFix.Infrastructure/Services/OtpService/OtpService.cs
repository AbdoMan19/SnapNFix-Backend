using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;

using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.OtpService;

public class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _otpLifetime = TimeSpan.FromMinutes(5);

    public OtpService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateOtpAsync(string phoneNumber)
    {
        string otp = GenerateRandomOtp();
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _otpLifetime
        };

        await _cache.SetStringAsync($"OTP_{phoneNumber}", otp, options);
        
        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
    {
        string storedOtp = await _cache.GetStringAsync($"OTP_{phoneNumber}");
        
        if (string.IsNullOrEmpty(storedOtp))
            return false;
            
        return storedOtp == otp;
    }

    public async Task InvalidateOtpAsync(string phoneNumber)
    {
        await _cache.RemoveAsync($"OTP_{phoneNumber}");
    }

    private string GenerateRandomOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }
}