using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Options;

namespace SnapNFix.Application.Features.Statistics.Queries.GetCategoryDistribution;

public class GetCategoryDistributionQueryHandler : IRequestHandler<GetCategoryDistributionQuery, GenericResponseModel<List<CategoryDistributionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetCategoryDistributionQueryHandler> _logger;

    public GetCategoryDistributionQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetCategoryDistributionQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<List<CategoryDistributionDto>>> Handle(
        GetCategoryDistributionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {

            var cached = await _cacheService.GetAsync<List<CategoryDistributionDto>>(CacheKeys.CategoryDistribution);
            if (cached != null)
            {
                return GenericResponseModel<List<CategoryDistributionDto>>.Success(cached);
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

            await _cacheService.SetAsync(CacheKeys.CategoryDistribution, result, TimeSpan.FromMinutes(5));
            return GenericResponseModel<List<CategoryDistributionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category distribution");
            return GenericResponseModel<List<CategoryDistributionDto>>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(ex), Shared.OperationFailed)
                });
        }
    }
}