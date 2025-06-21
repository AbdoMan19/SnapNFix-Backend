using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQueryHandler : 
    IRequestHandler<GetNearbyIssuesQuery, GenericResponseModel<List<NearbyIssueDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetNearbyIssuesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<List<NearbyIssueDto>>> Handle(
        GetNearbyIssuesQuery request, CancellationToken cancellationToken)
    {
        if (request.NorthEastLat <= request.SouthWestLat || 
            request.NorthEastLng <= request.SouthWestLng)
        {
            return GenericResponseModel<List<NearbyIssueDto>>.Failure(
                "Invalid viewport bounds: NorthEast coordinates must be greater than SouthWest coordinates");
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
            }).ToListAsync();

        return GenericResponseModel<List<NearbyIssueDto>>.Success(issuesInViewport);
    }
}