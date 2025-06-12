using FluentValidation;

namespace SnapNFix.Application.Features.FastReport.Create;

public class CreateFastReportCommandValidator : AbstractValidator<CreateFastReportCommand>
{
    public CreateFastReportCommandValidator()
    {
      
        RuleFor(x => x.IssueId)
            .NotEmpty().WithMessage("Issue ID is required.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(500).WithMessage("Comment must not exceed 500 characters.");
    }
}