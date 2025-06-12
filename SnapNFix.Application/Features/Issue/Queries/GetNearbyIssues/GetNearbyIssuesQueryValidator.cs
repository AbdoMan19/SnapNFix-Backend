using FluentValidation;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetNearbyIssuesQueryValidator : AbstractValidator<GetNearbyIssuesQuery>
{
    public GetNearbyIssuesQueryValidator()
    {
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

        RuleFor(x => x.Radius)
            .GreaterThan(0)
            .WithMessage("Radius must be greater than zero.")
            .LessThanOrEqualTo(10)
            .WithMessage("Radius must not exceed 10 kilometers.");
    }
}