using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Users.Commands.SubscribeToCity
{
    public class SubscribeToCityChannelCommand : IRequest<GenericResponseModel<bool>>
    {
        public Guid CityId { get; set; }
    }
}