using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.DTOs;

namespace SnapNFix.Application.Features.Users.Queries.GetUserSubscribedCitiesChannel;

public class GetUserSubscribedCitiesChannelCommand : IRequest<GenericResponseModel<PagedList<UserCityChannelSubscriptionDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5; 
}