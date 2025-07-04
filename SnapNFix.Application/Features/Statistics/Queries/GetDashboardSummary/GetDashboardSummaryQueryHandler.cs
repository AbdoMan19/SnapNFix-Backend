using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Options;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, GenericResponseModel<StatisticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetDashboardSummaryQueryHandler> _logger;

    public GetDashboardSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetDashboardSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<StatisticsDto>> Handle(
        GetDashboardSummaryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _cacheService.GetAsync<StatisticsDto>(CacheKeys.DashboardSummary);
            if (cached != null)
            {
                return GenericResponseModel<StatisticsDto>.Success(cached);
            }

            _logger.LogInformation("Generating fast dashboard summary");

            var summary = new StatisticsDto
            {
                Metrics = await GetMetricsAsync(cancellationToken),
                MonthlyTarget = await GetMonthlyTargetAsync(cancellationToken)
            };

            await _cacheService.SetAsync(CacheKeys.DashboardSummary, summary, TimeSpan.FromMinutes(5));

            return GenericResponseModel<StatisticsDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return GenericResponseModel<StatisticsDto>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Error", Shared.OperationFailed)
                });
        }
    }

    private async Task<MetricsOverviewDto> GetMetricsAsync(CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<MetricsOverviewDto>(CacheKeys.MetricsOverview);
        if (cached != null)
        {
            return cached;
        }

        var issueRepo = _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

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

        await _cacheService.SetAsync(CacheKeys.MetricsOverview, metrics, TimeSpan.FromMinutes(5));
        return metrics;
    }

    private async Task<MonthlyTargetDto> GetMonthlyTargetAsync(CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<MonthlyTargetDto>(CacheKeys.MonthlyTarget);
        if (cached != null)
        {
            return cached;
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

            await _cacheService.SetAsync(CacheKeys.MonthlyTarget, defaultTarget, TimeSpan.FromMinutes(5));
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

        await _cacheService.SetAsync(CacheKeys.MonthlyTarget, target, TimeSpan.FromMinutes(5));
        return target;
    }
}