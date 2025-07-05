using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Options;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Statistics.Queries.GetMetrics;
using SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Statistics.Queries.GetStatistics;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, GenericResponseModel<StatisticsDto>>
{
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetStatisticsQueryHandler> _logger;

    public GetStatisticsQueryHandler(
        IMediator mediator,
        ICacheService cacheService,
        ILogger<GetStatisticsQueryHandler> logger)
    {
        _mediator = mediator;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<StatisticsDto>> Handle(
        GetStatisticsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {

            var cached = await _cacheService.GetAsync<StatisticsDto>(CacheKeys.FullStatistics);
            if (cached != null)
            {
                return GenericResponseModel<StatisticsDto>.Success(cached);
            }

            _logger.LogInformation("Generating full statistics");

            var metricsResult = await _mediator.Send(new GetMetricsQuery(), cancellationToken);
            var monthlyTargetResult = await _mediator.Send(new GetMonthlyTargetQuery(), cancellationToken);

            if (metricsResult.ErrorList.Count > 0 || monthlyTargetResult.ErrorList.Count > 0)
            {
                _logger.LogError("Error retrieving statistics components");
                return GenericResponseModel<StatisticsDto>.Failure(Shared.OperationFailed,
                    metricsResult.ErrorList.Concat(monthlyTargetResult.ErrorList).ToList()
                    );
            }

            var statistics = new StatisticsDto
            {
                Metrics = metricsResult.Data,
                MonthlyTarget = monthlyTargetResult.Data
            };

            await _cacheService.SetAsync(CacheKeys.FullStatistics, statistics, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Full statistics generated and cached");
            return GenericResponseModel<StatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return GenericResponseModel<StatisticsDto>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(ex), Shared.OperationFailed)
                });
        }
    }
}