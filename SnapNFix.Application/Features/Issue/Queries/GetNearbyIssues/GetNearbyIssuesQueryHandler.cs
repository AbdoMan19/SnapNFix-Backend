using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
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
        var userLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        
        var issues = await _unitOfWork.Repository<Domain.Entities.Issue>()
            .GetQuerableData()
            .Where(i => i.Location.Distance(userLocation) <= request.Radius)
            .ToListAsync(cancellationToken);

        var issueDtos = issues.Select(issue => new NearbyIssueDto
        {
            Id = issue.Id,
            Latitude = issue.Location.Y,
            Longitude = issue.Location.X
        }).ToList();
        
        return GenericResponseModel<List<NearbyIssueDto>>.Success(issueDtos);
    }
}