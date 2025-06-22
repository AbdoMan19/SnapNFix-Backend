using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, GenericResponseModel<StatisticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetDashboardSummaryQueryHandler> _logger;
    private const int FastCacheMinutes = 2;

    public GetDashboardSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<GetDashboardSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<StatisticsDto>> Handle(
        GetDashboardSummaryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string cacheKey = "dashboard_summary";

            if (_cache.TryGetValue(cacheKey, out StatisticsDto cachedSummary))
            {
                return GenericResponseModel<StatisticsDto>.Success(cachedSummary);
            }

            _logger.LogInformation("Generating fast dashboard summary");

            var summary = new StatisticsDto
            {
                Metrics = await GetMetricsAsync(cancellationToken),
                MonthlyTarget = await GetMonthlyTargetAsync(cancellationToken)
            };

            _cache.Set(cacheKey, summary, TimeSpan.FromMinutes(FastCacheMinutes));

            return GenericResponseModel<StatisticsDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return GenericResponseModel<StatisticsDto>.Failure("An error occurred while retrieving dashboard summary");
        }
    }

    private async Task<MetricsOverviewDto> GetMetricsAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "metrics_overview";

        if (_cache.TryGetValue(cacheKey, out MetricsOverviewDto cachedMetrics))
        {
            return cachedMetrics;
        }

        var issueRepo = _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        // Execute queries sequentially to avoid DbContext concurrency issues
        var totalIncidents = await issueRepo.GetQuerableData()
            .CountAsync(cancellationToken);

        var resolvedIncidents = await issueRepo.FindBy(i => i.Status == IssueStatus.Completed)
            .CountAsync(cancellationToken);

        var pendingIncidents = await issueRepo.FindBy(i => i.Status == IssueStatus.Pending)
            .CountAsync(cancellationToken);

        var newThisMonth = await issueRepo.FindBy(i => i.CreatedAt >= startOfMonth)
            .CountAsync(cancellationToken);

        var newLastMonth = await issueRepo.FindBy(i => i.CreatedAt >= startOfLastMonth && i.CreatedAt < startOfMonth)
            .CountAsync(cancellationToken);

        var resolvedThisMonth = await issueRepo.FindBy(i => i.Status == IssueStatus.Completed && i.CreatedAt >= startOfMonth)
            .CountAsync(cancellationToken);

        var resolvedLastMonth = await issueRepo.FindBy(i => i.Status == IssueStatus.Completed && i.CreatedAt >= startOfLastMonth && i.CreatedAt < startOfMonth)
            .CountAsync(cancellationToken);

        var pendingThisMonth = await issueRepo.FindBy(i => i.Status == IssueStatus.Pending && i.CreatedAt >= startOfMonth)
            .CountAsync(cancellationToken);

        var pendingLastMonth = await issueRepo.FindBy(i => i.Status == IssueStatus.Pending && i.CreatedAt >= startOfLastMonth && i.CreatedAt < startOfMonth)
            .CountAsync(cancellationToken);

        var resolutionRate = totalIncidents > 0 ? Math.Round((double)resolvedIncidents / totalIncidents * 100, 2) : 0;
        var monthlyGrowthPercentage = newLastMonth > 0 ? Math.Round((double)(newThisMonth - newLastMonth) / newLastMonth * 100, 2) : 0;
        var resolvedIncidentsChange = resolvedLastMonth > 0 ? Math.Round((double)(resolvedThisMonth - resolvedLastMonth) / resolvedLastMonth * 100, 2) : 0;
        var pendingIncidentsChange = pendingLastMonth > 0 ? Math.Round((double)(pendingThisMonth - pendingLastMonth) / pendingLastMonth * 100, 2) : 0;

        var metrics = new MetricsOverviewDto
        {
            TotalIncidents = totalIncidents,
            ResolvedIncidents = resolvedIncidents,
            PendingIncidents = pendingIncidents,
            ResolutionRate = resolutionRate,
            NewThisMonth = newThisMonth,
            MonthlyGrowthPercentage = monthlyGrowthPercentage,
            ResolvedIncidentsChange = resolvedIncidentsChange,
            PendingIncidentsChange = pendingIncidentsChange
        };

        _cache.Set(cacheKey, metrics, TimeSpan.FromMinutes(FastCacheMinutes));
        return metrics;
    }

    private async Task<MonthlyTargetDto> GetMonthlyTargetAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "monthly_target";

        if (_cache.TryGetValue(cacheKey, out MonthlyTargetDto cachedTarget))
        {
            return cachedTarget;
        }

        const double targetResolutionRate = 95.0;

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyStats = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
            .FindBy(i => i.CreatedAt >= startOfMonth)
            .GroupBy(i => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Resolved = g.Count(i => i.Status == IssueStatus.Completed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (monthlyStats == null || monthlyStats.Total == 0)
        {
            var defaultTarget = new MonthlyTargetDto
            {
                TargetResolutionRate = targetResolutionRate,
                CurrentResolutionRate = 0,
                Progress = 0,
                IncidentsToTarget = 0,
                Status = "No Data",
                Percentage = 0,
                Target = targetResolutionRate,
                Current = 0,
                Improvement = 0
            };

            _cache.Set(cacheKey, defaultTarget, TimeSpan.FromMinutes(FastCacheMinutes));
            return defaultTarget;
        }

        var currentResolutionRate = Math.Round((double)monthlyStats.Resolved / monthlyStats.Total * 100, 2);
        var progress = Math.Round(currentResolutionRate / targetResolutionRate * 100, 2);
        var incidentsToTarget = Math.Max(0, (int)Math.Ceiling(monthlyStats.Total * (targetResolutionRate / 100)) - monthlyStats.Resolved);

        var status = currentResolutionRate >= targetResolutionRate ? "Ahead" :
                    currentResolutionRate >= targetResolutionRate * 0.9 ? "On Track" : "Behind";

        var target = new MonthlyTargetDto
        {
            TargetResolutionRate = targetResolutionRate,
            CurrentResolutionRate = currentResolutionRate,
            Progress = Math.Min(progress, 100),
            IncidentsToTarget = incidentsToTarget,
            Status = status,
            Percentage = currentResolutionRate,
            Target = targetResolutionRate,
            Current = currentResolutionRate,
            Improvement = Math.Round(currentResolutionRate - targetResolutionRate, 2)
        };

        _cache.Set(cacheKey, target, TimeSpan.FromMinutes(FastCacheMinutes));
        return target;
    }
}