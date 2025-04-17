using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.Logout;

public class LogoutCommand : IRequest<GenericResponseModel<bool>>
{

}