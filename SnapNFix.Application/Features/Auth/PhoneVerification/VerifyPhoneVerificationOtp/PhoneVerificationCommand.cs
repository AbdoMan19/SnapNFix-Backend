using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;

public sealed record PhoneVerificationCommand
    (
        string Otp
    ):IRequest<GenericResponseModel<string>>;