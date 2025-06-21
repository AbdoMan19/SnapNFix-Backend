using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Statistics.Queries.GetGeographicDistribution;

public class GetGeographicDistributionQueryHandler : IRequestHandler<GetGeographicDistributionQuery, GenericResponseModel<List<GeographicDistributionDto>>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetGeographicDistributionQueryHandler> _logger;

    public GetGeographicDistributionQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetGeographicDistributionQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<GeographicDistributionDto>>> Handle(
        GetGeographicDistributionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var geoData = await _statisticsService.GetGeographicDistributionAsync(
                request.Limit,  
                cancellationToken);
            return GenericResponseModel<List<GeographicDistributionDto>>.Success(geoData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving geographic distribution");
            return GenericResponseModel<List<GeographicDistributionDto>>.Failure("An error occurred while retrieving geographic distribution");
        }
    }
}
