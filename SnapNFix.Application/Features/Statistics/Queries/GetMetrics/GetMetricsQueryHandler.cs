using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMetrics;

public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, GenericResponseModel<MetricsOverviewDto>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetMetricsQueryHandler> _logger;

    public GetMetricsQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetMetricsQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<MetricsOverviewDto>> Handle(
        GetMetricsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var metrics = await _statisticsService.GetMetricsAsync(cancellationToken);
            return GenericResponseModel<MetricsOverviewDto>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return GenericResponseModel<MetricsOverviewDto>.Failure("An error occurred while retrieving metrics");
        }
    }
}