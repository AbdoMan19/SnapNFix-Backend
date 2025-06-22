using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;

public class GetIncidentTrendsQueryHandler : IRequestHandler<GetIncidentTrendsQuery, GenericResponseModel<List<IncidentTrendDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetIncidentTrendsQueryHandler> _logger;
    private const int SlowCacheMinutes = 15;

    public GetIncidentTrendsQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<GetIncidentTrendsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<IncidentTrendDto>>> Handle(
        GetIncidentTrendsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"incident_trends_{request.Interval}";

            if (_cache.TryGetValue(cacheKey, out List<IncidentTrendDto> cachedTrends))
            {
                return GenericResponseModel<List<IncidentTrendDto>>.Success(cachedTrends);
            }

            var trends = request.Interval switch
            {
                StatisticsInterval.Monthly => await GetMonthlyTrendsAsync(cancellationToken),
                StatisticsInterval.Quarterly => await GetQuarterlyTrendsAsync(cancellationToken),
                StatisticsInterval.Yearly => await GetYearlyTrendsAsync(cancellationToken),
                _ => await GetMonthlyTrendsAsync(cancellationToken)
            };

            _cache.Set(cacheKey, trends, TimeSpan.FromMinutes(SlowCacheMinutes));
            return GenericResponseModel<List<IncidentTrendDto>>.Success(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident trends for interval {Interval}", request.Interval);
            return GenericResponseModel<List<IncidentTrendDto>>.Failure("An error occurred while retrieving incident trends");
        }
    }

    private async Task<List<IncidentTrendDto>> GetMonthlyTrendsAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddMonths(-11).Date;
        var endDate = DateTime.UtcNow.Date;

        // Convert to UTC to avoid timezone issues
        var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc); // Add 1 day to include the end date

        var monthlyData = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
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

        var quarterlyData = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
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

        var yearlyData = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
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
}