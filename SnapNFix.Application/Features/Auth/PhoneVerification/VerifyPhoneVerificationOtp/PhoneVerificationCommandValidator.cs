using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;

public class PhoneVerificationCommandValidator : AbstractValidator<PhoneVerificationCommand>
{
    public PhoneVerificationCommandValidator()
    {
        RuleFor(x => x.Otp)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(Shared.OtpRequired)
            .Length(6).WithMessage(Shared.InvalidOtpLength)
            .Matches("^[0-9]+$").WithMessage(Shared.InvalidOtpFormat);

    }
}