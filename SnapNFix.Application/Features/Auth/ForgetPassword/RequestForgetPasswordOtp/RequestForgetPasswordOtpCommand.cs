using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class RequestForgetPasswordOtpCommand : IRequest<GenericResponseModel<bool>>
{
    public string EmailOrPhoneNumber { get; set; }
}