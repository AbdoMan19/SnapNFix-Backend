using FluentValidation;

namespace SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;

public class CreateSnapReportCommandValidator : AbstractValidator<CreateSnapReportCommand>
{
    public CreateSnapReportCommandValidator()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .WithMessage("Comment cannot exceed 500 characters.");

        RuleFor(x => x.Image)
            .NotEmpty()
            .WithMessage("Image required.");

        RuleFor(x => x.Latitude)
            .NotEmpty()
            .WithMessage("Latitude is required.")
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .NotEmpty()
            .WithMessage("Longitude is required.")
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.Severity)
            .IsInEnum()
            .WithMessage("Invalid severity level.");
        RuleFor(x => x.Road)
            .NotEmpty()
            .WithMessage("Road is required.")
            .MaximumLength(200)
            .WithMessage("Road cannot exceed 200 characters.");
        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters.");
        RuleFor(x => x.State)
            .MaximumLength(100)
            .WithMessage("State cannot exceed 100 characters.");
        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters.");
    }
}