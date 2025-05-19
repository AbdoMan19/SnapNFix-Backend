using FluentValidation;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;

public class ImageValidationResultCommandValidator : AbstractValidator<ImageValidationResultCommand>
{
    public ImageValidationResultCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required.");

        RuleFor(x => x.ImageStatus)
            .IsInEnum()
            .WithMessage("Invalid image status.");

        RuleFor(x => x.ReportCategory)
            .IsInEnum()
            .WithMessage("Invalid report category.");

        RuleFor(x => x.Threshold)
            .InclusiveBetween(0, 1)
            .WithMessage("Threshold must be between 0 and 1.");
    }
}