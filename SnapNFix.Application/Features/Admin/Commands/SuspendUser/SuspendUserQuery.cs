using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Admin.Commands.SuspendUser;


public class SuspendUserQuery : IRequest<GenericResponseModel<bool>>
{
    public Guid UserId { get; set; }
    public bool IsSuspended { get; set; }
}