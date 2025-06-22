using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;

public class GetMonthlyTargetQueryHandler : IRequestHandler<GetMonthlyTargetQuery, GenericResponseModel<MonthlyTargetDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetMonthlyTargetQueryHandler> _logger;
    private const int FastCacheMinutes = 2;

    public GetMonthlyTargetQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<GetMonthlyTargetQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<MonthlyTargetDto>> Handle(
        GetMonthlyTargetQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string cacheKey = "monthly_target";

            if (_cache.TryGetValue(cacheKey, out MonthlyTargetDto cachedTarget))
            {
                return GenericResponseModel<MonthlyTargetDto>.Success(cachedTarget);
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
                return GenericResponseModel<MonthlyTargetDto>.Success(defaultTarget);
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
            return GenericResponseModel<MonthlyTargetDto>.Success(target);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly target");
            return GenericResponseModel<MonthlyTargetDto>.Failure(Shared.OperationFailed);
        }
    }
}
