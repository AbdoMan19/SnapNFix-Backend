using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StatisticsService> _logger;
    private const int FastCacheMinutes = 2;
    private const int SlowCacheMinutes = 15;
    private const int GeoCacheMinutes = 60;

    public StatisticsService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<StatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<StatisticsDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "dashboard_summary";

        if (_cache.TryGetValue(cacheKey, out StatisticsDto cachedSummary))
        {
            return cachedSummary;
        }

        _logger.LogInformation("Generating fast dashboard summary");

        var summary = new StatisticsDto
        {
            Metrics = await GetMetricsAsync(cancellationToken),
            MonthlyTarget = await GetMonthlyTargetAsync(cancellationToken)
        };

        _cache.Set(cacheKey, summary, TimeSpan.FromMinutes(FastCacheMinutes));

        return summary;
    }

    public async Task<MetricsOverviewDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "metrics_overview";

        if (_cache.TryGetValue(cacheKey, out MetricsOverviewDto cachedMetrics))
        {
            return cachedMetrics;
        }

        var issueRepo = _unitOfWork.Repository<Issue>();

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

    public async Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "category_distribution";

        if (_cache.TryGetValue(cacheKey, out List<CategoryDistributionDto> cachedCategories))
        {
            return cachedCategories;
        }

        var categoryStats = await _unitOfWork.Repository<Issue>()
            .GetQuerableData()
            .GroupBy(i => i.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Count(),
                Resolved = g.Count(i => i.Status == IssueStatus.Completed),
                Pending = g.Count(i => i.Status == IssueStatus.Pending)
            })
            .ToListAsync(cancellationToken);

        var totalIssues = categoryStats.Sum(c => c.Total);

        var result = categoryStats.Select(c => new CategoryDistributionDto
        {
            Category = c.Category.ToString(),
            Total = c.Total,
            Resolved = c.Resolved,
            Pending = c.Pending,
            Percentage = totalIssues > 0 ? Math.Round((double)c.Total / totalIssues * 100, 2) : 0
        }).OrderByDescending(c => c.Total).ToList();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(SlowCacheMinutes));
        return result;
    }

    public async Task<MonthlyTargetDto> GetMonthlyTargetAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "monthly_target";

        if (_cache.TryGetValue(cacheKey, out MonthlyTargetDto cachedTarget))
        {
            return cachedTarget;
        }

        const double targetResolutionRate = 95.0;

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyStats = await _unitOfWork.Repository<Issue>()
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

    public async Task<List<IncidentTrendDto>> GetIncidentTrendsAsync(StatisticsInterval interval = StatisticsInterval.Monthly, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"incident_trends_{interval}";

        if (_cache.TryGetValue(cacheKey, out List<IncidentTrendDto> cachedTrends))
        {
            return cachedTrends;
        }

        var trends = interval switch
        {
            StatisticsInterval.Monthly => await GetMonthlyTrendsAsync(cancellationToken),
            StatisticsInterval.Quarterly => await GetQuarterlyTrendsAsync(cancellationToken),
            StatisticsInterval.Yearly => await GetYearlyTrendsAsync(cancellationToken),
            _ => await GetMonthlyTrendsAsync(cancellationToken)
        };

        _cache.Set(cacheKey, trends, TimeSpan.FromMinutes(SlowCacheMinutes));
        return trends;
    }

    public async Task<StatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "full_statistics";

        if (_cache.TryGetValue(cacheKey, out StatisticsDto cachedStats))
        {
            return cachedStats;
        }

        _logger.LogInformation("Generating full statistics");

        var statistics = new StatisticsDto
        {
            Metrics = await GetMetricsAsync(cancellationToken),
            MonthlyTarget = await GetMonthlyTargetAsync(cancellationToken)
        };

        _cache.Set(cacheKey, statistics, TimeSpan.FromMinutes(SlowCacheMinutes));

        _logger.LogInformation("Full statistics generated and cached");
        return statistics;
    }

    private async Task<List<IncidentTrendDto>> GetMonthlyTrendsAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddMonths(-11).Date;
        var endDate = DateTime.UtcNow.Date;

        // Convert to UTC to avoid timezone issues
        var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc); // Add 1 day to include the end date

        var monthlyData = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.CreatedAt >= startDateUtc && i.CreatedAt < endDateUtc)
            .GroupBy(i => new { Year = i.CreatedAt.Year, Month = i.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalIncidents = g.Count(),
                ResolvedIncidents = g.Count(i => i.Status == IssueStatus.Completed),
                PendingIncidents = g.Count(i => i.Status == IssueStatus.Pending)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return monthlyData.Select(m => new IncidentTrendDto
        {
            Period = new DateTime(m.Year, m.Month, 1).ToString("MMM"),
            TotalIncidents = m.TotalIncidents,
            ResolvedIncidents = m.ResolvedIncidents,
            PendingIncidents = m.PendingIncidents,
            Date = new DateTime(m.Year, m.Month, 1)
        }).ToList();
    }

    private async Task<List<IncidentTrendDto>> GetQuarterlyTrendsAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddYears(-2).Date;
        var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

        var quarterlyData = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.CreatedAt >= startDateUtc)
            .ToListAsync(cancellationToken);

        return quarterlyData
            .GroupBy(i => new
            {
                Year = i.CreatedAt.Year,
                Quarter = (i.CreatedAt.Month - 1) / 3 + 1
            })
            .Select(g => new IncidentTrendDto
            {
                Period = $"Q{g.Key.Quarter} {g.Key.Year}",
                TotalIncidents = g.Count(),
                ResolvedIncidents = g.Count(i => i.Status == IssueStatus.Completed),
                PendingIncidents = g.Count(i => i.Status == IssueStatus.Pending),
                Date = new DateTime(g.Key.Year, (g.Key.Quarter - 1) * 3 + 1, 1)
            })
            .OrderBy(q => q.Date)
            .ToList();
    }

    private async Task<List<IncidentTrendDto>> GetYearlyTrendsAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddYears(-5).Date;
        var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

        var yearlyData = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.CreatedAt >= startDateUtc)
            .GroupBy(i => i.CreatedAt.Year)
            .Select(g => new
            {
                Year = g.Key,
                TotalIncidents = g.Count(),
                ResolvedIncidents = g.Count(i => i.Status == IssueStatus.Completed),
                PendingIncidents = g.Count(i => i.Status == IssueStatus.Pending)
            })
            .OrderBy(g => g.Year)
            .ToListAsync(cancellationToken);

        return yearlyData.Select(y => new IncidentTrendDto
        {
            Period = y.Year.ToString(),
            TotalIncidents = y.TotalIncidents,
            ResolvedIncidents = y.ResolvedIncidents,
            PendingIncidents = y.PendingIncidents,
            Date = new DateTime(y.Year, 1, 1)
        }).ToList();
    }

    public async Task<List<GeographicDistributionDto>> GetGeographicDistributionAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var cityData = await _unitOfWork.Repository<Issue>()
            .GetQuerableData()
            .Where(i => !string.IsNullOrEmpty(i.City))
            .GroupBy(i => new { i.City })
            .Select(g => new
            {
                City = g.Key.City,
                IncidentCount = g.Count(),
            })
            .OrderByDescending(g => g.IncidentCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return cityData.Select(g => new GeographicDistributionDto
        {
            City = g.City,
            IncidentCount = g.IncidentCount
        }).ToList();
    }
}