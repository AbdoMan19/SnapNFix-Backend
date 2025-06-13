using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQuery : IRequest<GenericResponseModel<List<NearbyIssueDto>>>
{
    public double NorthEastLat { get; set; }
    public double NorthEastLng { get; set; }
    public double SouthWestLat { get; set; }
    public double SouthWestLng { get; set; }
    public int MaxResults { get; set; } = 100;
}