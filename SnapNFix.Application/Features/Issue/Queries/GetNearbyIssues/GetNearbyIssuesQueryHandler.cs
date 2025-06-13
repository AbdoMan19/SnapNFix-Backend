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
        var latDegreeDistance = request.Radius / 111.0;
        var lonDegreeDistance = request.Radius / (111.0 * Math.Cos(request.Latitude * Math.PI / 180.0));

        var minLat = request.Latitude - latDegreeDistance;
        var maxLat = request.Latitude + latDegreeDistance;
        var minLon = request.Longitude - lonDegreeDistance;
        var maxLon = request.Longitude + lonDegreeDistance;

        var issuesInBoundingBox = await _unitOfWork.Repository<Domain.Entities.Issue>()
            .GetQuerableData()
            .Where(i => i.Location.Y >= minLat && i.Location.Y <= maxLat &&
                       i.Location.X >= minLon && i.Location.X <= maxLon)
            .ToListAsync(cancellationToken);

        // Calculate exact distances and filter by radius
        var radiusInMeters = request.Radius * 1000;
        var nearbyIssuesWithDistance = issuesInBoundingBox
            .Select(issue => new
            {
                Issue = issue,
                Distance = CalculateDistanceInMeters(
                    request.Latitude, request.Longitude,
                    issue.Location.Y, issue.Location.X)
            })
            .Where(x => x.Distance <= radiusInMeters)
            .OrderBy(x => x.Distance)
            .Take(100)
            .ToList();

        var issueDtos = nearbyIssuesWithDistance.Select(x => new NearbyIssueDto
        {
            Id = x.Issue.Id,
            Latitude = x.Issue.Location.Y,
            Longitude = x.Issue.Location.X
        }).ToList();
        
        return GenericResponseModel<List<NearbyIssueDto>>.Success(issueDtos);
    }


    private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return earthRadiusKm * c * 1000; 
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
