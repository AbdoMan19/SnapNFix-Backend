using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;

public class GetIncidentTrendsQueryHandler : IRequestHandler<GetIncidentTrendsQuery, GenericResponseModel<List<IncidentTrendDto>>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetIncidentTrendsQueryHandler> _logger;

    public GetIncidentTrendsQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetIncidentTrendsQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<IncidentTrendDto>>> Handle(
        GetIncidentTrendsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var trends = await _statisticsService.GetIncidentTrendsAsync(request.Interval, cancellationToken);
            return GenericResponseModel<List<IncidentTrendDto>>.Success(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident trends for interval {Interval}", request.Interval);
            return GenericResponseModel<List<IncidentTrendDto>>.Failure("An error occurred while retrieving incident trends");
        }
    }
}
