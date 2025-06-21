using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, GenericResponseModel<StatisticsDto>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetDashboardSummaryQueryHandler> _logger;

    public GetDashboardSummaryQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetDashboardSummaryQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<StatisticsDto>> Handle(
        GetDashboardSummaryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _statisticsService.GetDashboardSummaryAsync(cancellationToken);
            return GenericResponseModel<StatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return GenericResponseModel<StatisticsDto>.Failure("An error occurred while retrieving dashboard summary");
        }
    }
}
