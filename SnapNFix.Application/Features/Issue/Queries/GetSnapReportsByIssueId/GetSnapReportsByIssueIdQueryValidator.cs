using FluentValidation;

namespace SnapNFix.Application.Features.Issue.Queries.GetSnapReportsByIssueId;

public class GetSnapReportsByIssueIdQueryValidator : AbstractValidator<GetSnapReportsByIssueIdQuery>
{
    public GetSnapReportsByIssueIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Issue ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Issue ID must be a valid GUID.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than zero.");
    }
}
