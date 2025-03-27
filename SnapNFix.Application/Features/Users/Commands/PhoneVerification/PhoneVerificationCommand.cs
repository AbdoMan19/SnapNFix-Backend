using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;

namespace SnapNFix.Application.Features.Users.Commands.PhoneVerification;

public sealed record PhoneVerificationCommand
    (
        string PhoneNumber,
        string Otp
        ):IRequest<GenericResponseModel<AuthResponse>>;