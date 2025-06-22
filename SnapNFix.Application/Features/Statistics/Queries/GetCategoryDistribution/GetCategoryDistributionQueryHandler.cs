using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Statistics.Queries.GetCategoryDistribution;

public class GetCategoryDistributionQueryHandler : IRequestHandler<GetCategoryDistributionQuery, GenericResponseModel<List<CategoryDistributionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetCategoryDistributionQueryHandler> _logger;
    private const int SlowCacheMinutes = 15;

    public GetCategoryDistributionQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<GetCategoryDistributionQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<CategoryDistributionDto>>> Handle(
        GetCategoryDistributionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string cacheKey = "category_distribution";

            if (_cache.TryGetValue(cacheKey, out List<CategoryDistributionDto> cachedCategories))
            {
                return GenericResponseModel<List<CategoryDistributionDto>>.Success(cachedCategories);
            }

            var categoryStats = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
                .GetQuerableData()
                .GroupBy(i => i.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Count(),
                    Resolved = g.Count(i => i.Status == IssueStatus.Completed),
                    Pending = g.Count(i => i.Status == IssueStatus.Pending)
                })
                .ToListAsync(cancellationToken);

            var totalIssues = categoryStats.Sum(c => c.Total);

            var result = categoryStats.Select(c => new CategoryDistributionDto
            {
                Category = c.Category.ToString(),
                Total = c.Total,
                Resolved = c.Resolved,
                Pending = c.Pending,
                Percentage = totalIssues > 0 ? Math.Round((double)c.Total / totalIssues * 100, 2) : 0
            }).OrderByDescending(c => c.Total).ToList();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(SlowCacheMinutes));
            return GenericResponseModel<List<CategoryDistributionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category distribution");
            return GenericResponseModel<List<CategoryDistributionDto>>.Failure(Shared.OperationFailed);
        }
    }
}