using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IStatisticsService : IScoped
{
    Task<StatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task<List<IncidentTrendDto>> GetIncidentTrendsAsync(string interval = "monthly", CancellationToken cancellationToken = default);
    Task<List<GeographicDistributionDto>> GetGeographicDistributionAsync(int limit = 10, CancellationToken cancellationToken = default);
}