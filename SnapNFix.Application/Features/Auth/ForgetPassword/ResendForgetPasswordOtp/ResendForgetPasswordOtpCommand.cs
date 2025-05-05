using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.ResendForgetPasswordOtp;

public class ResendForgetPasswordOtpCommand : IRequest<GenericResponseModel<bool>>
{

}