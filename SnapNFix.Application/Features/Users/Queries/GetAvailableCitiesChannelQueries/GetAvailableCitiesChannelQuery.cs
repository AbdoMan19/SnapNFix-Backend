using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.DTOs;

namespace SnapNFix.Application.Features.Users.Queries.GetAvailableCitiesChannelQueries;

public class GetAvailableCitiesChannelQuery : IRequest<GenericResponseModel<PagedList<CityChannelSubscriptionDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}