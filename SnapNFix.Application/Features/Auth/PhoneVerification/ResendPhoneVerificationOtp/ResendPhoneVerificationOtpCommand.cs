using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.ResendPhoneVerificationOtp;

public class ResendPhoneVerificationOtpCommand : IRequest<GenericResponseModel<bool>>
{
}