using FluentValidation;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class ForgetPasswordCommandValidator : AbstractValidator<RequestForgetPasswordCommand>
{
    public ForgetPasswordCommandValidator()
    {
        RuleFor(f => f.EmailOrPhoneNumber)
            .NotEmpty()
            .WithMessage("Email or Phone Number is required.")
            .Matches(@"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)|(^(\+20|0)?1[0125][0-9]{8}$)")
            .WithMessage("Input must be a valid email or Egyptian phone number.");
    }
}