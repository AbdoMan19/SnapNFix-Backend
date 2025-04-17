using FluentValidation;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;

public class ForgetPasswordOtpCommandValidator : AbstractValidator<VerifyForgetPasswordOtpCommand>
{
    public ForgetPasswordOtpCommandValidator()
    {
        RuleFor(f => f.EmailOrPhoneNumber)
            .NotEmpty()
            .WithMessage("Email or Phone Number is required.")
            .Matches(@"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)|(^(\+20|0)?1[0125][0-9]{8}$)")
            .WithMessage("Input must be a valid email or Egyptian phone number.");
        RuleFor(f => f.Otp)
            .NotEmpty()
            .WithMessage("OTP is required.")
            .Matches(@"^\d{6}$")
            .WithMessage("OTP must be a 6-digit number.");
    }
}