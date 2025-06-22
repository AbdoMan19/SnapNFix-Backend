using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMetrics;

public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, GenericResponseModel<MetricsOverviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetMetricsQueryHandler> _logger;
    private const int FastCacheMinutes = 2;

    public GetMetricsQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<GetMetricsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<MetricsOverviewDto>> Handle(
        GetMetricsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string cacheKey = "metrics_overview";

            if (_cache.TryGetValue(cacheKey, out MetricsOverviewDto cachedMetrics))
            {
                return GenericResponseModel<MetricsOverviewDto>.Success(cachedMetrics);
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
            return GenericResponseModel<MetricsOverviewDto>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return GenericResponseModel<MetricsOverviewDto>.Failure(Shared.OperationFailed);
        }
    }
}
