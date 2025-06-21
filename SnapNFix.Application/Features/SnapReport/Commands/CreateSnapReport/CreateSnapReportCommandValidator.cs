using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;

public class CreateSnapReportCommandValidator : AbstractValidator<CreateSnapReportCommand>
{
    public CreateSnapReportCommandValidator()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .WithMessage(Shared.CommentMaxLength);

        RuleFor(x => x.Image)
            .NotEmpty()
            .WithMessage(Shared.ImageRequired);

        RuleFor(x => x.Latitude)
            .NotEmpty()
            .WithMessage(Shared.LatitudeRequired)
            .InclusiveBetween(-90, 90)
            .WithMessage(Shared.LatitudeRange);

        RuleFor(x => x.Longitude)
            .NotEmpty()
            .WithMessage(Shared.LongitudeRequired)
            .InclusiveBetween(-180, 180)
            .WithMessage(Shared.LongitudeRange);

        RuleFor(x => x.Severity)
            .IsInEnum()
            .WithMessage(Shared.InvalidSeverity);

        RuleFor(x => x.Road)
            .NotEmpty()
            .WithMessage(Shared.RoadRequired)
            .MaximumLength(200)
            .WithMessage(Shared.RoadMaxLength);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage(Shared.CityMaxLength);

        RuleFor(x => x.State)
            .MaximumLength(100)
            .WithMessage(Shared.StateMaxLength);

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage(Shared.CountryMaxLength);
    }
}