using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage(Shared.RefreshTokenRequired);
    }
    
}