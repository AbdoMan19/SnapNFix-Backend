using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Users.Commands.UnsubscribeFromCityChannel;

public class UnsubscribeFromCityChannelCommand : IRequest<GenericResponseModel<bool>>
{
    public Guid CityId { get; set; }
}