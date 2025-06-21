using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssueByIdQueryValidator : AbstractValidator<GetIssueByIdQuery>
{
    public GetIssueByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Shared.IssueIdRequired)
            .NotEqual(Guid.Empty)
            .WithMessage(Shared.InvalidIssueId);
    }
}