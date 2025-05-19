/*using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using SnapNFix.Api.RateLimiting.Interfaces;

namespace SnapNFix.Api.RateLimiting;

public class IpRateLimiter : IRateLimiter
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<IpRateLimiter> _logger;

    public IpRateLimiter(IDistributedCache cache, ILogger<IpRateLimiter> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsIpBlockedAsync(IPAddress ipAddress, string path, RateLimitRule rule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(rule);

        var key = $"ratelimit:{path}:{ipAddress}";
        var now = DateTime.UtcNow;

        try
        {
            var requestHistory = await GetRequestHistoryAsync(key, cancellationToken);
            requestHistory.RemoveAll(x => x < now.AddMinutes(-rule.WindowInMinutes));

            if (requestHistory.Count >= rule.Limit)
            {
                return true;
            }

            requestHistory.Add(now);
            await SaveRequestHistoryAsync(key, requestHistory, rule.WindowInMinutes, cancellationToken);

            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error checking rate limit for IP {IpAddress}", ipAddress);
            return false;
        }
    }

    private async Task<List<DateTime>> GetRequestHistoryAsync(string key, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetStringAsync(key, cancellationToken);
        return cached != null 
            ? JsonSerializer.Deserialize<List<DateTime>>(cached) ?? new List<DateTime>()
            : new List<DateTime>();
    }

    private async Task SaveRequestHistoryAsync(string key, List<DateTime> history, int windowMinutes, CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(windowMinutes)
        };

        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(history),
            options,
            cancellationToken
        );
    }
}*/