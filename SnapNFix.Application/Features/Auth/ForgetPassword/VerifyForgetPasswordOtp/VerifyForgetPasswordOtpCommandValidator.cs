using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class VerifyForgetPasswordOtpCommandValidator : AbstractValidator<VerifyForgetPasswordOtpCommand>
{
    public VerifyForgetPasswordOtpCommandValidator()
    {
        RuleFor(f => f.Otp)
            .NotEmpty()
            .WithMessage(Shared.OtpRequired)
            .Matches(@"^\d{6}$")
            .WithMessage(Shared.InvalidOtp);
    }
}