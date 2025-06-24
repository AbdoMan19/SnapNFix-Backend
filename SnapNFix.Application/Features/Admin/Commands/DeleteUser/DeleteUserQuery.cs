using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Admin.Commands.DeleteUser;

public class DeleteUserQuery : IRequest<GenericResponseModel<bool>>
{
    public Guid UserId { get; set; }
}