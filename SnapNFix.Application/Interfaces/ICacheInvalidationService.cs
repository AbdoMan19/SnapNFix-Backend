using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

public interface ICacheInvalidationService : IScoped
{
    Task InvalidateUserCacheAsync(Guid userId);
    Task InvalidateIssueCacheAsync(Guid issueId);
    Task InvalidateReportCacheAsync(Guid reportId, Guid? issueId = null);
    Task InvalidateStatisticsCacheAsync();
}