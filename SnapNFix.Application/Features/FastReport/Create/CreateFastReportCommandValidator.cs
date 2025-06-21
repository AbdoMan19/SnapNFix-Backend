using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.FastReport.Create;

public class CreateFastReportCommandValidator : AbstractValidator<CreateFastReportCommand>
{
    public CreateFastReportCommandValidator()
    {
        RuleFor(x => x.IssueId)
            .NotEmpty().WithMessage(Shared.IssueIdRequired);

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage(Shared.CommentRequired)
            .MaximumLength(500).WithMessage(Shared.CommentMaxLength);
    }
}