using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(Shared.PasswordRequired)
            .MinimumLength(8).WithMessage(Shared.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(Shared.PasswordMissingUpper)
            .Matches("[a-z]").WithMessage(Shared.PasswordMissingLower)
            .Matches("[0-9]").WithMessage(Shared.PasswordMissingNumber)
            .Matches("[^a-zA-Z0-9]").WithMessage(Shared.PasswordMissingSpecial);
    }
}