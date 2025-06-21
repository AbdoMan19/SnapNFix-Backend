using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Issue.Queries.GetFastReportsByIssueId;

public class GetFastReportsByIssueIdQueryValidator : AbstractValidator<GetFastReportsByIssueIdQuery>
{
    public GetFastReportsByIssueIdQueryValidator()
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