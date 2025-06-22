using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SnapNFix.Infrastructure.Services.CacheService;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentDictionary<string, object> _keyTracker = new();
    
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var cached) && cached is T typedValue)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return typedValue;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache item with key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            if (value == null) return;

            var cacheExpiration = expiration ?? DefaultExpiration;
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheExpiration,
                Priority = CacheItemPriority.Normal
            };

            _keyTracker.TryAdd(key, null);
            options.RegisterPostEvictionCallback((k, v, reason, state) =>
            {
                _keyTracker.TryRemove(k.ToString(), out _);
            });

            _memoryCache.Set(key, value, options);

            _logger.LogDebug("Cached item with key: {Key}, expiration: {Expiration}", key, cacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache item with key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out _);
            _logger.LogDebug("Removed cache item with key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache item with key: {Key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern)
    {
        try
        {
            var keysToRemove = _keyTracker.Keys
                .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }

            _logger.LogDebug("Removed {Count} cache items matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache items with pattern: {Pattern}", pattern);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
        {
            return cached;
        }

        var item = await getItem();
        if (item != null)
        {
            await SetAsync(key, item, expiration);
        }

        return item;
    }
}