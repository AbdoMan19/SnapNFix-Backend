using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Issue.Queries.GetSnapReportsByIssueId;

public class GetSnapReportsByIssueIdQueryValidator : AbstractValidator<GetSnapReportsByIssueIdQuery>
{
    public GetSnapReportsByIssueIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Shared.IssueIdRequired)
            .NotEqual(Guid.Empty)
            .WithMessage(Shared.InvalidIssueId);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage(Shared.InvalidPageNumber);
    }
}
