using MediatR;
using SnapNFix.Application.Common.ResponseModel;


public class SuspendUserQuery : IRequest<GenericResponseModel<bool>>
{
    public Guid UserId { get; set; }
    public bool IsSuspended { get; set; }
}