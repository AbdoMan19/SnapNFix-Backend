using SnapNFix.Domain.Interfaces.ServiceLifetime;

public interface ICacheInvalidationService : IScoped
{
    Task InvalidateUserCacheAsync(Guid userId);
    Task InvalidateIssueCacheAsync(Guid issueId);
    Task InvalidateReportCacheAsync(Guid reportId, Guid? issueId = null);
    Task InvalidateStatisticsCacheAsync();
}

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task InvalidateUserCacheAsync(Guid userId)
    {
        await _cacheService.RemovePatternAsync($"user:{userId}");
    }

    public async Task InvalidateIssueCacheAsync(Guid issueId)
    {
        await _cacheService.RemoveAsync(CacheKeys.IssueDetails(issueId));
        await _cacheService.RemovePatternAsync($"issue:reports:{issueId}");
        await _cacheService.RemovePatternAsync($"issue:fast-reports:{issueId}");
        await _cacheService.RemovePatternAsync("issues:list");
        await _cacheService.RemovePatternAsync("issues:nearby");
    }

    public async Task InvalidateReportCacheAsync(Guid reportId, Guid? issueId = null)
    {
        await _cacheService.RemoveAsync(CacheKeys.ReportDetails(reportId));
        
        if (issueId.HasValue)
        {
            await InvalidateIssueCacheAsync(issueId.Value);
        }
        
        await InvalidateStatisticsCacheAsync();
    }

    public async Task InvalidateStatisticsCacheAsync()
    {
        await _cacheService.RemovePatternAsync(CacheKeys.StatisticsPattern);
    }
}