using SnapNFix.Domain.Interfaces.ServiceLifetime;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Interfaces;

public interface IStatisticsService : IScoped
{
    Task<StatisticsDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<MetricsOverviewDto> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync(CancellationToken cancellationToken = default);
    Task<MonthlyTargetDto> GetMonthlyTargetAsync(CancellationToken cancellationToken = default);
    Task<List<IncidentTrendDto>> GetIncidentTrendsAsync(StatisticsInterval interval = StatisticsInterval.Monthly, CancellationToken cancellationToken = default);
    Task<List<GeographicDistributionDto>> GetGeographicDistributionAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<StatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}