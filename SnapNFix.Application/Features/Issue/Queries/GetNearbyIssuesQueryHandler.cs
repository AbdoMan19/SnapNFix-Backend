using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQueryHandler : 
    IRequestHandler<GetNearbyIssuesQuery, GenericResponseModel<List<IssueDetailsDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetNearbyIssuesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<List<IssueDetailsDto>>> Handle(
        GetNearbyIssuesQuery request, CancellationToken cancellationToken)
    {
        var userLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        
        var query = _unitOfWork.Repository<Domain.Entities.Issue>()
            .GetQuerableData()
            .Where(i => i.Location.Distance(userLocation) <= request.Radius);


        var issues = await query.ToListAsync(cancellationToken);
        var mappedItems = _mapper.Map<List<IssueDetailsDto>>(issues);
        
        
        return GenericResponseModel<List<IssueDetailsDto>>.Success(mappedItems);
    }
}