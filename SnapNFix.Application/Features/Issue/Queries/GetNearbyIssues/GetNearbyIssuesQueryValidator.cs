using FluentValidation;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQueryValidator : AbstractValidator<GetNearbyIssuesQuery>
{
    public GetNearbyIssuesQueryValidator()
    {
        RuleFor(x => x.NorthEastLat)
            .NotEmpty()
            .WithMessage("North East Latitude is required.")
            .InclusiveBetween(-90, 90)
            .WithMessage("North East Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.NorthEastLng)
            .NotEmpty()
            .WithMessage("North East Longitude is required.")
            .InclusiveBetween(-180, 180)
            .WithMessage("North East Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.SouthWestLat)
            .NotEmpty()
            .WithMessage("South West Latitude is required.")
            .InclusiveBetween(-90, 90)
            .WithMessage("South West Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.SouthWestLng)
            .NotEmpty()
            .WithMessage("South West Longitude is required.")
            .InclusiveBetween(-180, 180)
            .WithMessage("South West Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("MaxResults must be greater than zero.")
            .LessThanOrEqualTo(500)
            .WithMessage("MaxResults must not exceed 500 for performance reasons.");

        RuleFor(x => x)
            .Must(x => x.NorthEastLat > x.SouthWestLat)
            .WithMessage("North East Latitude must be greater than South West Latitude.")
            .OverridePropertyName("NorthEastLat");

        RuleFor(x => x)
            .Must(x => x.NorthEastLng > x.SouthWestLng)
            .WithMessage("North East Longitude must be greater than South West Longitude.")
            .OverridePropertyName("NorthEastLng");

        RuleFor(x => x)
            .Must(x => (x.NorthEastLat - x.SouthWestLat) <= 10)
            .WithMessage("Viewport height cannot exceed 10 degrees latitude.")
            .OverridePropertyName("ViewportSize");

        RuleFor(x => x)
            .Must(x => (x.NorthEastLng - x.SouthWestLng) <= 10)
            .WithMessage("Viewport width cannot exceed 10 degrees longitude.")
            .OverridePropertyName("ViewportSize");
    }
}