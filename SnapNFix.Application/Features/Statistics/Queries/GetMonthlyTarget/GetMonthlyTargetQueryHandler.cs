using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;

public class GetMonthlyTargetQueryHandler : IRequestHandler<GetMonthlyTargetQuery, GenericResponseModel<MonthlyTargetDto>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetMonthlyTargetQueryHandler> _logger;

    public GetMonthlyTargetQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetMonthlyTargetQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<MonthlyTargetDto>> Handle(
        GetMonthlyTargetQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var target = await _statisticsService.GetMonthlyTargetAsync(cancellationToken);
            return GenericResponseModel<MonthlyTargetDto>.Success(target);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly target");
            return GenericResponseModel<MonthlyTargetDto>.Failure("An error occurred while retrieving monthly target");
        }
    }
}
