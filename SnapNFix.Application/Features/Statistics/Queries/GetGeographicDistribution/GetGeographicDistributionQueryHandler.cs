using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Options;

namespace SnapNFix.Application.Features.Statistics.Queries.GetGeographicDistribution;

public class GetGeographicDistributionQueryHandler : IRequestHandler<GetGeographicDistributionQuery, GenericResponseModel<List<GeographicDistributionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetGeographicDistributionQueryHandler> _logger;

    public GetGeographicDistributionQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetGeographicDistributionQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<GeographicDistributionDto>>> Handle(
        GetGeographicDistributionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = CacheKeys.GeographicDistribution;

            var cached = await _cacheService.GetAsync<List<GeographicDistributionDto>>(cacheKey);
            if (cached != null)
            {
                return GenericResponseModel<List<GeographicDistributionDto>>.Success(cached);
            }

            var cityData = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
                .GetQuerableData()
                .Where(i => !string.IsNullOrEmpty(i.City))
                .GroupBy(i => new { i.State })
                .Select(g => new
                {
                    State = g.Key.State,
                    IncidentCount = g.Count(),
                })
                .OrderByDescending(g => g.IncidentCount)
                .ToListAsync(cancellationToken);

            var result = cityData.Select(g => new GeographicDistributionDto
            {
                State = g.State,
                IncidentCount = g.IncidentCount
            }).ToList();

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(60));
            return GenericResponseModel<List<GeographicDistributionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving geographic distribution");
            return GenericResponseModel<List<GeographicDistributionDto>>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(ex), Shared.OperationFailed)
                });
        }
    }
}