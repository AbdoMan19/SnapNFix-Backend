using FluentValidation;
using SnapNFix.Application.Resources;


public class SuspendUserQueryValidator : AbstractValidator<SuspendUserQuery>
{
    public SuspendUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Shared.UserNotFound)
            .NotEqual(Guid.Empty)
            .WithMessage(Shared.UserNotFound);
    }
}