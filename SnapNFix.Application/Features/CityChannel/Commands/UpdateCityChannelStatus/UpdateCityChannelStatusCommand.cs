using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.CityChannel.Commands.UpdateCityChannelStatus;

public class UpdateCityChannelStatusCommand: IRequest<GenericResponseModel<bool>>
{
    public Guid CityId { get; set; }
    public bool IsActive { get; set; }
}