using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommand : IRequest<GenericResponseModel<LoginResponse>>
{
    public string IdToken { get; set; }
    public string DeviceId { get; set; }
    
    public string DeviceName { get; set; }
    
    public string DeviceType { get; set; }
    
    public string Platform { get; set; }
}