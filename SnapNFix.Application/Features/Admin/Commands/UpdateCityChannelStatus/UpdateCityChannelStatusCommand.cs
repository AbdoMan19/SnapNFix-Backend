using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Admin.Commands.UpdateCityChannel;

public class UpdateCityChannelStatusCommand: IRequest<GenericResponseModel<bool>>
{
    public Guid CityId { get; set; }
    public bool IsActive { get; set; }
}