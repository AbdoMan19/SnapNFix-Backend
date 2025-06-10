using FluentValidation;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssueByIdQueryValidator : AbstractValidator<GetIssueByIdQuery>
{
    public GetIssueByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Issue ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Issue ID must be a valid GUID.");
    }
}