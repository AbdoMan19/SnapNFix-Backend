using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class ForgetPasswordOtpCommandHandler : IRequestHandler<VerifyForgetPasswordOtpCommand, GenericResponseModel<string>>
{
    public Task<GenericResponseModel<string>> Handle(VerifyForgetPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        
    }
}