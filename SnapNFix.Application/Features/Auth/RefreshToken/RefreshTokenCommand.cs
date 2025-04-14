using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<GenericResponseModel<AuthResponse>>
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string IpAddress { get; set; }
}