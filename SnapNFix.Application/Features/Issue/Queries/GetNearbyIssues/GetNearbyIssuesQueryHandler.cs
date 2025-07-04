using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Application.Options;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQueryHandler : 
    IRequestHandler<GetNearbyIssuesQuery, GenericResponseModel<List<NearbyIssueDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetNearbyIssuesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<GenericResponseModel<List<NearbyIssueDto>>> Handle(
        GetNearbyIssuesQuery request, CancellationToken cancellationToken)
    {
        if (request.NorthEastLat <= request.SouthWestLat || 
            request.NorthEastLng <= request.SouthWestLng)
        {
            return GenericResponseModel<List<NearbyIssueDto>>.Failure(
                Shared.InvalidCoordinates,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.NorthEastLat), Shared.InvalidCoordinates),
                    ErrorResponseModel.Create(nameof(request.NorthEastLng), Shared.InvalidCoordinates),
                    ErrorResponseModel.Create(nameof(request.SouthWestLat), Shared.InvalidCoordinates),
                    ErrorResponseModel.Create(nameof(request.SouthWestLng), Shared.InvalidCoordinates)
                });
        }

        var cacheKey = CacheKeys.NearbyIssues(request.NorthEastLat, request.NorthEastLng, request.SouthWestLat, request.SouthWestLng);
        
        var cached = await _cacheService.GetAsync<List<NearbyIssueDto>>(cacheKey);
        if (cached != null)
        {
            return GenericResponseModel<List<NearbyIssueDto>>.Success(cached);
        }

        var issuesInViewport = await _unitOfWork.Repository<Domain.Entities.Issue>()
            .GetQuerableData()
            .Where(i => i.Location.Y >= request.SouthWestLat && 
                       i.Location.Y <= request.NorthEastLat &&
                       i.Location.X >= request.SouthWestLng && 
                       i.Location.X <= request.NorthEastLng)
            .OrderBy(i => i.CreatedAt) 
            .Take(request.MaxResults)
            .Select(i => new NearbyIssueDto
            {
                Id = i.Id,
                Latitude = i.Location.Y,
                Longitude = i.Location.X
            })
            .ToListAsync(cancellationToken);

        await _cacheService.SetAsync(cacheKey, issuesInViewport, TimeSpan.FromMinutes(5));

        return GenericResponseModel<List<NearbyIssueDto>>.Success(issuesInViewport);
    }
}