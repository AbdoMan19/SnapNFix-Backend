using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommand : IRequest<GenericResponseModel<AuthResponse>>
{
    public string IdToken { get; set; }
}