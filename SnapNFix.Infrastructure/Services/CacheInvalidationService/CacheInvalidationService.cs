
using SnapNFix.Application.Common.Options;

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
        await _cacheService.RemoveAsync(CacheKeys.FullStatistics);
        await _cacheService.RemoveAsync(CacheKeys.DashboardSummary);
        await _cacheService.RemoveAsync(CacheKeys.MonthlyTarget);
        await _cacheService.RemoveAsync(CacheKeys.MetricsOverview);
        await _cacheService.RemoveAsync(CacheKeys.CategoryDistribution);
        await _cacheService.RemoveAsync(CacheKeys.GeographicDistribution);
        await _cacheService.RemoveAsync(CacheKeys.IncidentTrends(StatisticsInterval.Monthly));
        await _cacheService.RemoveAsync(CacheKeys.IncidentTrends(StatisticsInterval.Quarterly));
        await _cacheService.RemoveAsync(CacheKeys.IncidentTrends(StatisticsInterval.Yearly));
    }

    public Task InvalidateMonthlyTargetCacheAsync()
    {
        return _cacheService.RemoveAsync(CacheKeys.MonthlyTarget);
    }
}