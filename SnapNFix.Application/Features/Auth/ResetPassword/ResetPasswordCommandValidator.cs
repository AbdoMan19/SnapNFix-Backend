using FluentValidation;

namespace SnapNFix.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {

        RuleFor(f => f.EmailOrPhoneNumber)
            .NotEmpty()
            .WithMessage("Email or Phone Number is required.")
            .Matches(@"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)|(^(\+20|0)?1[0125][0-9]{8}$)")
            .WithMessage("Input must be a valid email or Egyptian phone number.");


        RuleFor(x => x.Token)
            .NotEmpty();
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}