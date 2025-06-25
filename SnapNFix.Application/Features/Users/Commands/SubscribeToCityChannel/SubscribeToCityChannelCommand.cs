using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Users.Commands.SubscribeToCityChannel
{
    public class SubscribeToCityChannelCommand : IRequest<GenericResponseModel<bool>>
    {
        public Guid CityId { get; set; }
    }
}