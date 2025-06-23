using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Interfaces;
using SnapNFix.Infrastructure.Options;

namespace SnapNFix.Infrastructure.Services.OtpService;


public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OtpService> _logger;
    private readonly OtpOptions _options;
    private const string AttemptCountKeySuffix = ":attempts";

    public OtpService(
        IMemoryCache cache,
        ILogger<OtpService> logger,
        IOptions<OtpOptions> options)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<string> GenerateOtpAsync(string emailOrPhoneNumber, OtpPurpose otpPurpose)
    {
        if (string.IsNullOrWhiteSpace(emailOrPhoneNumber))
            throw new ArgumentException("Email or phone number cannot be empty", nameof(emailOrPhoneNumber));

        // if (IsRateLimitExceeded(emailOrPhoneNumber, otpPurpose))
        // {
        //     _logger.LogWarning("Rate limit exceeded for {Identifier} with purpose {Purpose}", 
        //         emailOrPhoneNumber, otpPurpose);
        //     throw new InvalidOperationException("Rate limit exceeded. Please try again later.");
        // }

        string otp = GenerateRandomOtp();
        var cacheKey = GenerateCacheKey(emailOrPhoneNumber, otpPurpose);
        var attemptsKey = $"{cacheKey}{AttemptCountKeySuffix}";
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.OtpLifetimeMinutes));

        _cache.Set(cacheKey, otp, cacheEntryOptions);
        _cache.Set(attemptsKey, 0, cacheEntryOptions); // Reset attempt counter
        
        _logger.LogInformation("OTP generated for {Identifier} with purpose {Purpose}", 
            emailOrPhoneNumber, otpPurpose);
        
        await Task.CompletedTask; 
        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string emailOrPhoneNumber, string otp, OtpPurpose purpose)
    {
        if (string.IsNullOrWhiteSpace(emailOrPhoneNumber))
            throw new ArgumentException("Email or phone number cannot be empty", nameof(emailOrPhoneNumber));
        
        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP cannot be empty", nameof(otp));

        var cacheKey = GenerateCacheKey(emailOrPhoneNumber, purpose);
        var attemptsKey = $"{cacheKey}{AttemptCountKeySuffix}";
        
        if (_cache.TryGetValue(attemptsKey, out int attempts) && 
            attempts >= _options.MaxVerificationAttempts)
        {
            _logger.LogWarning("Max verification attempts reached for {Identifier} with purpose {Purpose}", 
                emailOrPhoneNumber, purpose);
            
            await InvalidateOtpAsync(emailOrPhoneNumber, purpose);
            return false;
        }
        
        _cache.Set(attemptsKey, (attempts + 1), 
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                TimeSpan.FromMinutes(_options.OtpLifetimeMinutes)));

        if (_cache.TryGetValue(cacheKey, out string storedOtp) && 
            storedOtp != null && 
            string.Equals(storedOtp, otp, StringComparison.Ordinal))
        {
            _logger.LogInformation("OTP verified successfully for {Identifier} with purpose {Purpose}", 
                emailOrPhoneNumber, purpose);
            
            if (_options.InvalidateOtpOnSuccessfulVerification)
            {
                await InvalidateOtpAsync(emailOrPhoneNumber, purpose);
            }
            
            return true;
        }
        
        _logger.LogWarning("OTP verification failed for {Identifier} with purpose {Purpose}, attempt {Attempt}", 
            emailOrPhoneNumber, purpose, attempts);
        
        return false;
    }


    public async Task InvalidateOtpAsync(string emailOrPhoneNumber, OtpPurpose purpose)
    {
        if (string.IsNullOrWhiteSpace(emailOrPhoneNumber))
            throw new ArgumentException("Email or phone number cannot be empty", nameof(emailOrPhoneNumber));

        var cacheKey = GenerateCacheKey(emailOrPhoneNumber, purpose);
        var attemptsKey = $"{cacheKey}{AttemptCountKeySuffix}";

        _cache.Remove(cacheKey);
        _cache.Remove(attemptsKey);
        
        _logger.LogInformation("OTP invalidated for {Identifier} with purpose {Purpose}", 
            emailOrPhoneNumber, purpose);
        
        await Task.CompletedTask; 
    }


    private string GenerateRandomOtp()
    {
        return "123456";
        // return RandomNumberGenerator.GetInt32(
        //     _options.MinOtpValue, 
        //     _options.MaxOtpValue).ToString($"D{_options.OtpLength}");
    }


    private static string GenerateCacheKey(string phoneOrEmail, OtpPurpose purpose)
    {
        return $"OTP:{purpose}:{phoneOrEmail}";
    }

    // private bool IsRateLimitExceeded(string emailOrPhoneNumber, OtpPurpose purpose)
    // {
    //     string rateLimitKey = $"RateLimit:{purpose}:{emailOrPhoneNumber}";
        
    //     if (!_cache.TryGetValue(rateLimitKey, out int requestCount))
    //     {
    //         _cache.Set(rateLimitKey, 1, TimeSpan.FromMinutes(_options.RateLimitWindowMinutes));
    //         return false;
    //     }
        
    //     if (requestCount >= _options.MaxRequestsPerWindow)
    //     {
    //         return true;
    //     }
        
    //     _cache.Set(rateLimitKey, requestCount + 1, TimeSpan.FromMinutes(_options.RateLimitWindowMinutes));
    //     return false;
    // }
}
