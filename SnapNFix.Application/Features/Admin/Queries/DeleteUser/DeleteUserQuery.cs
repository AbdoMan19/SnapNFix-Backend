using MediatR;
using SnapNFix.Application.Common.ResponseModel;

public class DeleteUserQuery : IRequest<GenericResponseModel<bool>>
{
    public Guid UserId { get; set; }
}