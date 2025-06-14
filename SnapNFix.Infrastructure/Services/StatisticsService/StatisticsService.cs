using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StatisticsService> _logger;
    private const int CacheExpirationMinutes = 15;

    public StatisticsService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<StatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<StatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "Statistics_statistics";

        if (_cache.TryGetValue(cacheKey, out StatisticsDto cachedStats))
        {
            return cachedStats;
        }

        _logger.LogInformation("Generating statistics");

        var statistics = new StatisticsDto
        {
            Metrics = await GetMetricsOverviewAsync(cancellationToken),
            CategoryDistribution = await GetCategoryDistributionAsync(cancellationToken),
            MonthlyTarget = await GetMonthlyTargetAsync(cancellationToken),
            IncidentTrends = await GetIncidentTrendsAsync("monthly", cancellationToken)
        };

        _cache.Set(cacheKey, statistics, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        _logger.LogInformation("Statistics generated and cached");
        return statistics;
    }

    private async Task<MetricsOverviewDto> GetMetricsOverviewAsync(CancellationToken cancellationToken)
    {
        var snapReportRepo = _unitOfWork.Repository<SnapReport>();
        var issueRepo = _unitOfWork.Repository<Issue>();

        // Get current month boundaries
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        // Parallel execution for better performance
        var tasks = new[]
        {
            // Total approved incidents (reports that became issues)
            snapReportRepo.FindBy(r => r.ImageStatus == ImageStatus.Approved && r.IssueId != null)
                .CountAsync(cancellationToken),
            
            // Resolved incidents
            issueRepo.FindBy(i => i.Status == IssueStatus.Completed)
                .CountAsync(cancellationToken),
            
            // Pending incidents
            issueRepo.FindBy(i => i.Status == IssueStatus.Pending)
                .CountAsync(cancellationToken),
            
            // New incidents this month
            issueRepo.FindBy(i => i.CreatedAt >= startOfMonth)
                .CountAsync(cancellationToken),
            
            // New incidents last month
            issueRepo.FindBy(i => i.CreatedAt >= startOfLastMonth && i.CreatedAt < startOfMonth)
                .CountAsync(cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        
        var totalIncidents = results[0];
        var resolvedIncidents = results[1];
        var pendingIncidents = results[2];
        var newThisMonth = results[3];
        var newLastMonth = results[4];

        var resolutionRate = totalIncidents > 0 ? Math.Round((double)resolvedIncidents / totalIncidents * 100, 2) : 0;
        var monthlyGrowthPercentage = newLastMonth > 0 ? Math.Round((double)(newThisMonth - newLastMonth) / newLastMonth * 100, 2) : 0;

        return new MetricsOverviewDto
        {
            TotalIncidents = totalIncidents,
            ResolvedIncidents = resolvedIncidents,
            PendingIncidents = pendingIncidents,
            ResolutionRate = resolutionRate,
            NewThisMonth = newThisMonth,
            MonthlyGrowthPercentage = monthlyGrowthPercentage
        };
    }

    private async Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync(CancellationToken cancellationToken)
    {
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

        return categoryStats.Select(c => new CategoryDistributionDto
        {
            Category = c.Category.ToString(),
            Total = c.Total,
            Resolved = c.Resolved,
            Pending = c.Pending,
            Percentage = totalIssues > 0 ? Math.Round((double)c.Total / totalIssues * 100, 2) : 0
        }).OrderByDescending(c => c.Total).ToList();
    }

    private async Task<MonthlyTargetDto> GetMonthlyTargetAsync(CancellationToken cancellationToken)
    {
        const double targetResolutionRate = 95.0;
        
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

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
            return new MonthlyTargetDto
            {
                TargetResolutionRate = targetResolutionRate,
                CurrentResolutionRate = 0,
                Progress = 0,
                IncidentsToTarget = 0,
                Status = "No Data"
            };
        }

        var currentResolutionRate = Math.Round((double)monthlyStats.Resolved / monthlyStats.Total * 100, 2);
        var progress = Math.Round(currentResolutionRate / targetResolutionRate * 100, 2);
        var incidentsToTarget = Math.Max(0, (int)Math.Ceiling(monthlyStats.Total * (targetResolutionRate / 100)) - monthlyStats.Resolved);
        
        var status = currentResolutionRate >= targetResolutionRate ? "Ahead" :
                    currentResolutionRate >= targetResolutionRate * 0.9 ? "On Track" : "Behind";

        return new MonthlyTargetDto
        {
            TargetResolutionRate = targetResolutionRate,
            CurrentResolutionRate = currentResolutionRate,
            Progress = Math.Min(progress, 100),
            IncidentsToTarget = incidentsToTarget,
            Status = status
        };
    }

    public async Task<List<IncidentTrendDto>> GetIncidentTrendsAsync(string interval = "monthly", CancellationToken cancellationToken = default)
    {
        var cacheKey = $"incident_trends_{interval.ToLower()}";
        
        if (_cache.TryGetValue(cacheKey, out List<IncidentTrendDto> cachedTrends))
        {
            return cachedTrends;
        }

        var trends = interval.ToLower() switch
        {
            "monthly" => await GetMonthlyTrendsAsync(cancellationToken),
            "quarterly" => await GetQuarterlyTrendsAsync(cancellationToken),
            "yearly" => await GetYearlyTrendsAsync(cancellationToken),
            _ => await GetMonthlyTrendsAsync(cancellationToken)
        };

        _cache.Set(cacheKey, trends, TimeSpan.FromMinutes(CacheExpirationMinutes));
        return trends;
    }

    private async Task<List<IncidentTrendDto>> GetMonthlyTrendsAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddMonths(-11).Date; // Last 12 months
        var endDate = DateTime.UtcNow.Date;

        var monthlyData = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
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
        var startDate = DateTime.UtcNow.AddYears(-2).Date; // Last 2 years

        var quarterlyData = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.CreatedAt >= startDate)
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
        var startDate = DateTime.UtcNow.AddYears(-5).Date; // Last 5 years

        var yearlyData = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.CreatedAt >= startDate)
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
        const string cacheKey = "geographic_distribution";
        
        if (_cache.TryGetValue(cacheKey, out List<GeographicDistributionDto> cachedGeoData))
        {
            return cachedGeoData.Take(limit).ToList();
        }

        var geoData = await _unitOfWork.Repository<Issue>()
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

        var result = geoData.Select(g => new GeographicDistributionDto
        {
            City = g.City,
            IncidentCount = g.IncidentCount,
        }).ToList();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        return result;
    }
}