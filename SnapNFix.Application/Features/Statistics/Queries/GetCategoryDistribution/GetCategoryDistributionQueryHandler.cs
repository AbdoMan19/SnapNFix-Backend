using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetCategoryDistribution;

public class GetCategoryDistributionQueryHandler : IRequestHandler<GetCategoryDistributionQuery, GenericResponseModel<List<CategoryDistributionDto>>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<GetCategoryDistributionQueryHandler> _logger;

    public GetCategoryDistributionQueryHandler(
        IStatisticsService statisticsService,
        ILogger<GetCategoryDistributionQueryHandler> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<CategoryDistributionDto>>> Handle(
        GetCategoryDistributionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _statisticsService.GetCategoryDistributionAsync(cancellationToken);
            return GenericResponseModel<List<CategoryDistributionDto>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category distribution");
            return GenericResponseModel<List<CategoryDistributionDto>>.Failure("An error occurred while retrieving category distribution");
        }
    }
}
