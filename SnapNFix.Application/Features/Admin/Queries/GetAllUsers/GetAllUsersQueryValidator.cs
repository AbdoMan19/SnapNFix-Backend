using FluentValidation;
using SnapNFix.Application.Resources;

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage(Shared.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage(Shared.InvalidPageSize)
            .LessThanOrEqualTo(50)
            .WithMessage("Page size cannot exceed 50");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage(Shared.InvalidUserType)
            .When(x => x.Role.HasValue);;
    }
}