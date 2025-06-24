using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Admin.Queries.DeleteUser;

public class DeleteUserQuery : IRequest<GenericResponseModel<bool>>
{
    public Guid UserId { get; set; }
}