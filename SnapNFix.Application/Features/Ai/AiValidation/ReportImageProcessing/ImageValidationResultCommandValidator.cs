using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;

public class ImageValidationResultCommandValidator : AbstractValidator<ImageValidationResultCommand>
{
    public ImageValidationResultCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage(Shared.TaskIdRequired);

        RuleFor(x => x.ImageStatus)
            .IsInEnum()
            .WithMessage(Shared.InvalidImageStatus);

        RuleFor(x => x.ReportCategory)
            .IsInEnum()
            .WithMessage(Shared.InvalidReportCategory);

        RuleFor(x => x.Threshold)
            .InclusiveBetween(0, 1)
            .WithMessage(Shared.InvalidThreshold);
    }
}