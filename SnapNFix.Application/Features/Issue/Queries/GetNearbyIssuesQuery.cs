using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQuery : IRequest<GenericResponseModel<List<IssueDetailsDto>>>
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; } = 100; 
}