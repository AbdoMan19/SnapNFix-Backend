using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.CityChannel.DTOs;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.CityChannel.Queries.GetCityChannelIssues;

public class GetCityChannelIssuesQuery : IRequest<GenericResponseModel<PagedList<CityIssueDto>>>
{
    public Guid CityId { get; set; }
    public IssueStatus? StatusFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}