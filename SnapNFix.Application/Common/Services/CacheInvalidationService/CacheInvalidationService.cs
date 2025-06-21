using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SnapNFix.Application.Services.CacheInvalidationService;

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(
        ICacheService cacheService,
        ILogger<CacheInvalidationService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvalidateCacheAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating cache for key: {Key}", key);
        await _cacheService.RemoveAsync(key, cancellationToken);
    }

    public async Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating cache for pattern: {Pattern}", pattern);
        await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
    }
    public async Task InvalidateAllCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating all cache entries");
        await _cacheService.RemoveByPatternAsync("*", cancellationToken);
    }
}