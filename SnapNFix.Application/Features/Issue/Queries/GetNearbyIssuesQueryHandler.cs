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
        
        var query = _unitOfWork.Repository<Domain.Entities.Issue>()
            .GetQuerableData()
            .Where(i => i.Location.Distance(userLocation) <= request.Radius)
            .Select(i => new NearbyIssueDto
            {
                Id = i.Id,
                Latitude = i.Location.Y,
                Longitude = i.Location.X
            });

        var issues = await query.ToListAsync(cancellationToken);
        
        return GenericResponseModel<List<NearbyIssueDto>>.Success(issues);
    }
}