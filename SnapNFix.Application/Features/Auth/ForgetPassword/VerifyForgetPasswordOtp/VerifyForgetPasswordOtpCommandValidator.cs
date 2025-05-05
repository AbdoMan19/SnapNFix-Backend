using FluentValidation;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class VerifyForgetPasswordOtpCommandValidator : AbstractValidator<VerifyForgetPasswordOtpCommand>
{
    public VerifyForgetPasswordOtpCommandValidator()
    {
        RuleFor(f => f.Otp)
            .NotEmpty()
            .WithMessage("OTP is required.")
            .Matches(@"^\d{6}$")
            .WithMessage("OTP must be a 6-digit number.");
    }
}