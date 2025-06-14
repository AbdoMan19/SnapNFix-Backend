using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommand : IRequest<GenericResponseModel<LoginResponse>>
{
    public string EmailOrPhoneNumber { get; set; }
    
    public string Password { get; set; }
    
    public string DeviceId { get; set; }
    
    public string DeviceName { get; set; }
    
    public string DeviceType { get; set; }
    
    public string Platform { get; set; }
    public string FCMToken { get; set; } = string.Empty;
}