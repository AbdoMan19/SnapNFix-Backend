using FluentValidation;
using SnapNFix.Application.Resources;

public class DeleteUserQueryValidator : AbstractValidator<DeleteUserQuery>
{
    public DeleteUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Shared.UserNotFound)
            .NotEqual(Guid.Empty)
            .WithMessage(Shared.UserNotFound);
    }
}