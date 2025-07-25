﻿using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Options;

namespace SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;

public class GetIncidentTrendsQueryHandler : IRequestHandler<GetIncidentTrendsQuery, GenericResponseModel<List<IncidentTrendDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetIncidentTrendsQueryHandler> _logger;

    public GetIncidentTrendsQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetIncidentTrendsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<IncidentTrendDto>>> Handle(
        GetIncidentTrendsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = CacheKeys.IncidentTrends(request.Interval);

            var cached = await _cacheService.GetAsync<List<IncidentTrendDto>>(cacheKey);
            if (cached != null)
            {
                return GenericResponseModel<List<IncidentTrendDto>>.Success(cached);
            }

            var trends = request.Interval switch
            {
                StatisticsInterval.Monthly => await GetMonthlyTrendsAsync(cancellationToken),
                StatisticsInterval.Quarterly => await GetQuarterlyTrendsAsync(cancellationToken),
                StatisticsInterval.Yearly => await GetYearlyTrendsAsync(cancellationToken),
                _ => await GetMonthlyTrendsAsync(cancellationToken)
            };

            await _cacheService.SetAsync(cacheKey, trends, TimeSpan.FromMinutes(15));
            return GenericResponseModel<List<IncidentTrendDto>>.Success(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident trends for interval {Interval}", request.Interval);
            return GenericResponseModel<List<IncidentTrendDto>>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Interval", Shared.OperationFailed)
                });
        }
    }

    private async Task<List<IncidentTrendDto>> GetMonthlyTrendsAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddMonths(-11).Date;
        var endDate = DateTime.UtcNow.Date;

        // Convert to UTC to avoid timezone issues
        var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);

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