using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class VerifyForgetPasswordOtpCommand : IRequest<GenericResponseModel<string>>
{
    public string EmailOrPhoneNumber { get; set; }
    public string Otp { get; set; }
}