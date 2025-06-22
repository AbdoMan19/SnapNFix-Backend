using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Statistics.Queries.GetMetrics;
using SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetStatistics;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, GenericResponseModel<StatisticsDto>>
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetStatisticsQueryHandler> _logger;
    private const int SlowCacheMinutes = 15;

    public GetStatisticsQueryHandler(
        IMediator mediator,
        IMemoryCache cache,
        ILogger<GetStatisticsQueryHandler> logger)
    {
        _mediator = mediator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<StatisticsDto>> Handle(
        GetStatisticsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string cacheKey = "full_statistics";

            if (_cache.TryGetValue(cacheKey, out StatisticsDto cachedStats))
            {
                return GenericResponseModel<StatisticsDto>.Success(cachedStats);
            }

            _logger.LogInformation("Generating full statistics");

            // Use existing handlers to get the data
            var metricsResult = await _mediator.Send(new GetMetricsQuery(), cancellationToken);
            var monthlyTargetResult = await _mediator.Send(new GetMonthlyTargetQuery(), cancellationToken);

            if (metricsResult.ErrorList.Count > 0 || monthlyTargetResult.ErrorList.Count > 0)
            {
                _logger.LogError("Error retrieving statistics components");
                return GenericResponseModel<StatisticsDto>.Failure("An error occurred while retrieving statistics");
            }

            var statistics = new StatisticsDto
            {
                Metrics = metricsResult.Data,
                MonthlyTarget = monthlyTargetResult.Data
            };

            _cache.Set(cacheKey, statistics, TimeSpan.FromMinutes(SlowCacheMinutes));

            _logger.LogInformation("Full statistics generated and cached");
            return GenericResponseModel<StatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return GenericResponseModel<StatisticsDto>.Failure("An error occurred while retrieving statistics");
        }
    }
}