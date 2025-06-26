using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.CityChannel.DTOs;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.CityChannel.Queries.GetCityChannelMetrics;

public class GetCityChannelMetricsQueryHandler : IRequestHandler<GetCityChannelMetricsQuery, GenericResponseModel<CityMetricsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCityChannelMetricsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetCityChannelMetricsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCityChannelMetricsQueryHandler> logger,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<GenericResponseModel<CityMetricsDto>> Handle(
        GetCityChannelMetricsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check cache first
            var cacheKey = $"CityMetrics:{request.CityId}";
            var cached = await _cacheService.GetAsync<CityMetricsDto>(cacheKey);
            if (cached != null)
            {
                return GenericResponseModel<CityMetricsDto>.Success(cached);
            }

            // Verify city exists
            var cityChannel = await _unitOfWork.Repository<Domain.Entities.CityChannel>()
                .FindBy(cc => cc.Id == request.CityId)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (cityChannel == null)
            {
                return GenericResponseModel<CityMetricsDto>.Failure(
                    Shared.CityNotFound,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.CityId), Shared.CityNotFound)
                    });
            }
            
            // Get issues for this city
            var issues = await _unitOfWork.Repository<Domain.Entities.Issue>()
                .FindBy(i => i.City == cityChannel.Name && i.State == cityChannel.State)
                .ToListAsync(cancellationToken);
            
            // Calculate metrics by issue status
            var pendingCount = issues.Count(i => i.Status == IssueStatus.Pending);
            var inProgressCount = issues.Count(i => i.Status == IssueStatus.InProgress);
            var fixedCount = issues.Count(i => i.Status == IssueStatus.Completed);
            var totalIssues = issues.Count;
            
            // Calculate health percentage
            int healthPercentage;
            if (totalIssues > 0)
            {
                // Calculate health percentage based on fixed issues versus total issues
                // Add a weight for in-progress issues
                double completionScore = fixedCount + (inProgressCount * 0.5); 
                healthPercentage = (int)Math.Round((completionScore / totalIssues) * 100);
            }
            else
            {
                healthPercentage = 100; 
            }

            HealthCondition healthCondition = healthPercentage switch
            {
                >= 85 => HealthCondition.Excellent,
                >= 70 => HealthCondition.Good,
                >= 50 => HealthCondition.Average,
                _ => HealthCondition.Poor
            };

            // Get subscriber count
            var subscriberCount = await _unitOfWork.Repository<UserCitySubscription>().
                GetQuerableData()
                .CountAsync(s => s.CityChannelId == request.CityId, cancellationToken);
            
            // Create response
            var result = new CityMetricsDto
            {
                HealthPercentage = healthPercentage,
                HealthCondition = healthCondition,
                PendingIssuesCount = pendingCount,
                InProgressIssuesCount = inProgressCount,
                FixedIssuesCount = fixedCount,
                TotalSubscribers = subscriberCount
            };
            
            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            
            return GenericResponseModel<CityMetricsDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving city metrics for {CityId}", request.CityId);
            return GenericResponseModel<CityMetricsDto>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.CityId), Shared.OperationFailed)
                });
        }
    }
}