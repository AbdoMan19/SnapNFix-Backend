using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetGeographicDistribution;

public class GetGeographicDistributionQueryHandler : IRequestHandler<GetGeographicDistributionQuery, GenericResponseModel<List<GeographicDistributionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetGeographicDistributionQueryHandler> _logger;
    private const int GeoCacheMinutes = 60;

    public GetGeographicDistributionQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<GetGeographicDistributionQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<GeographicDistributionDto>>> Handle(
        GetGeographicDistributionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"geographic_distribution_{request.Limit}";

            if (_cache.TryGetValue(cacheKey, out List<GeographicDistributionDto> cachedGeoData))
            {
                return GenericResponseModel<List<GeographicDistributionDto>>.Success(cachedGeoData);
            }

            var cityData = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
                .GetQuerableData()
                .Where(i => !string.IsNullOrEmpty(i.City))
                .GroupBy(i => new { i.City })
                .Select(g => new
                {
                    City = g.Key.City,
                    IncidentCount = g.Count(),
                })
                .OrderByDescending(g => g.IncidentCount)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            var result = cityData.Select(g => new GeographicDistributionDto
            {
                City = g.City,
                IncidentCount = g.IncidentCount
            }).ToList();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(GeoCacheMinutes));
            return GenericResponseModel<List<GeographicDistributionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving geographic distribution");
            return GenericResponseModel<List<GeographicDistributionDto>>.Failure(Shared.OperationFailed);
        }
    }
}