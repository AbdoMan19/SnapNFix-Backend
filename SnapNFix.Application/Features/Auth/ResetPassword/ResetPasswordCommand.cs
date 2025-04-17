using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommand : IRequest<GenericResponseModel<bool>>
{
    public string EmailOrPhoneNumber { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
