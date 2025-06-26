using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommand : IRequest<GenericResponseModel<LoginResponse>>
{
    public string IdToken { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string FCMToken { get; set; } = string.Empty;
}