using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class VerifyForgetPasswordOtpCommand : IRequest<GenericResponseModel<string>>
{
    public string Otp { get; set; }
}