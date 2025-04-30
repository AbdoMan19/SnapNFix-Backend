using FluentValidation;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;

public class PhoneVerificationCommandValidator : AbstractValidator<PhoneVerificationCommand>
{
    public PhoneVerificationCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+20|0)?1[0125][0-9]{8}$").WithMessage("{PropertyName} must be a valid Egyptian phone number");

        RuleFor(x => x.Otp)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("OTP is required")
            .Length(6).WithMessage("OTP must be 6 digits")
            .Matches("^[0-9]+$").WithMessage("OTP must contain only numbers");
            
        RuleFor(x => x.VerificationToken)
            .NotEmpty().WithMessage("Verification token is required");
    }
}