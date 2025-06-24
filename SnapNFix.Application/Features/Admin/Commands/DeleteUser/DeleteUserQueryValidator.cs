using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Admin.Commands.DeleteUser;

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