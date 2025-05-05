using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.RequestPhoneVerificationOtp;

public class RequestPhoneVerificationOtpCommand : IRequest<GenericResponseModel<string>>
{
    public string PhoneNumber { get; set; }
}