using System.Data;
using FluentValidation;
using SnapNFix.Application.Features.Issue.Queries;
using SnapNFix.Application.Resources;

public class GetIssuesQueryValidator : AbstractValidator<GetIssuesQuery>
{
  public GetIssuesQueryValidator()
  {
    RuleFor(x => x.PageNumber)
      .GreaterThanOrEqualTo(1)
      .WithMessage(Shared.InvalidPageNumber);
    RuleFor(x => x.PageSize)
      .GreaterThanOrEqualTo(1)
      .WithMessage(Shared.InvalidPageSize)
      .LessThanOrEqualTo(50)
      .WithMessage(Shared.InvalidPageSize);
    RuleFor(x => x.Status)
      .IsInEnum()
      .WithMessage(Shared.InvalidIssueStatus)
      .When(x => x.Status.HasValue);
    RuleFor(x => x.Severity)
      .IsInEnum()
      .WithMessage(Shared.InvalidSeverity)
      .When(x => x.Severity.HasValue);
    RuleFor(x => x.Category)
      .IsInEnum()
      .WithMessage(Shared.InvalidReportCategory)
      .When(x => x.Category.HasValue);
  }
}