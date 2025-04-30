using FluentValidation;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.RequestPhoneVerificationOtp;

public class RequestPhoneVerificationOtpCommandValidator : AbstractValidator<RequestPhoneVerificationOtpCommand>
{
    public RequestPhoneVerificationOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+20|0)?1[0125][0-9]{8}$").WithMessage("{PropertyName} must be a valid Egyptian phone number");
    }
}