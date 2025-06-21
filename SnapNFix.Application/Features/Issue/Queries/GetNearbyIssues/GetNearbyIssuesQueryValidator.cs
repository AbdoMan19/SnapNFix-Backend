using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQueryValidator : AbstractValidator<GetNearbyIssuesQuery>
{
    public GetNearbyIssuesQueryValidator()
    {
        RuleFor(x => x.NorthEastLat)
            .NotEmpty()
            .WithMessage(Shared.NorthEastLatRequired)
            .InclusiveBetween(-90, 90)
            .WithMessage(Shared.InvalidLatitudeRange);

        RuleFor(x => x.NorthEastLng)
            .NotEmpty()
            .WithMessage(Shared.NorthEastLngRequired)
            .InclusiveBetween(-180, 180)
            .WithMessage(Shared.InvalidLongitudeRange);

        RuleFor(x => x.SouthWestLat)
            .NotEmpty()
            .WithMessage(Shared.SouthWestLatRequired)
            .InclusiveBetween(-90, 90)
            .WithMessage(Shared.InvalidLatitudeRange);

        RuleFor(x => x.SouthWestLng)
            .NotEmpty()
            .WithMessage(Shared.SouthWestLngRequired)
            .InclusiveBetween(-180, 180)
            .WithMessage(Shared.InvalidLongitudeRange);

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage(Shared.InvalidMaxResults)
            .LessThanOrEqualTo(500)
            .WithMessage(Shared.MaxResultsExceeded);

        RuleFor(x => x)
            .Must(x => x.NorthEastLat > x.SouthWestLat)
            .WithMessage(Shared.NorthEastLatGreaterThanSouthWestLat)
            .OverridePropertyName("NorthEastLat");

        RuleFor(x => x)
            .Must(x => x.NorthEastLng > x.SouthWestLng)
            .WithMessage(Shared.NorthEastLngGreaterThanSouthWestLng)
            .OverridePropertyName("NorthEastLng");

        RuleFor(x => x)
            .Must(x => (x.NorthEastLat - x.SouthWestLat) <= 10)
            .WithMessage(Shared.ViewportHeightExceeded)
            .OverridePropertyName("ViewportSize");

        RuleFor(x => x)
            .Must(x => (x.NorthEastLng - x.SouthWestLng) <= 10)
            .WithMessage(Shared.ViewportWidthExceeded)
            .OverridePropertyName("ViewportSize");
    }
}