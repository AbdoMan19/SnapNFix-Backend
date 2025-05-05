using FluentValidation;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;

public class PhoneVerificationCommandValidator : AbstractValidator<PhoneVerificationCommand>
{
    public PhoneVerificationCommandValidator()
    {
        RuleFor(x => x.Otp)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("OTP is required")
            .Length(6).WithMessage("OTP must be 6 digits")
            .Matches("^[0-9]+$").WithMessage("OTP must contain only numbers");
        
    }
}