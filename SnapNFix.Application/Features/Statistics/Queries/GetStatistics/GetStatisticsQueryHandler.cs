using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetStatistics;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, GenericResponseModel<StatisticsDto>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetStatisticsQueryHandler> _logger;

    public GetStatisticsQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetStatisticsQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<StatisticsDto>> Handle(
        GetStatisticsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _statisticsService.GetStatisticsAsync(cancellationToken);
            return GenericResponseModel<StatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return GenericResponseModel<StatisticsDto>.Failure("An error occurred while retrieving statistics");
        }
    }
}
