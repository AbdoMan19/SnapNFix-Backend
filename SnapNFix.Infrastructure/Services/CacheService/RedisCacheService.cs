using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Infrastructure.Options;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SnapNFix.Infrastructure.Services.CacheService;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache distributedCache,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(cachedValue))
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = SnapNFix.Application.Common.Configuration.CacheConfiguration.TTL.SlidingExpiration
        };
        
        await _distributedCache.SetStringAsync(key, serializedValue, cacheOptions, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: The default IDistributedCache doesn't support pattern-based removal
        // This is a simplified implementation that logs a warning
        _logger.LogWarning("Pattern-based cache removal not supported in this implementation: {Pattern}", pattern);
        await Task.CompletedTask;
    }
}