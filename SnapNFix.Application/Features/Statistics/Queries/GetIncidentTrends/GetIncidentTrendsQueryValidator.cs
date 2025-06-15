using FluentValidation;

namespace SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;

public class GetIncidentTrendsQueryValidator : AbstractValidator<GetIncidentTrendsQuery>
{
    public GetIncidentTrendsQueryValidator()
    {
        RuleFor(x => x.Interval)
            .NotEmpty()
            .WithMessage("Interval is required")
            .Must(interval => new[] { "monthly", "quarterly", "yearly" }.Contains(interval.ToLower()))
            .WithMessage("Interval must be 'monthly', 'quarterly', or 'yearly'");
    }
}