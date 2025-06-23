using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Issue.Commands.UpdateIssue;

public class UpdateIssueCommandValidator : AbstractValidator<UpdateIssueCommand>
{
    public UpdateIssueCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Shared.IssueIdRequired)
            .NotEqual(Guid.Empty)
            .WithMessage(Shared.InvalidIssueId);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(Shared.InvalidIssueStatus)
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Severity)
            .IsInEnum()
            .WithMessage(Shared.InvalidSeverity)
            .When(x => x.Severity.HasValue);

        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage(Shared.NoChangesDetected)
            .OverridePropertyName("UpdateFields");
    }

    private static bool HaveAtLeastOneField(UpdateIssueCommand command)
    {
        return command.Status.HasValue ||
               command.Severity.HasValue;
    }
}