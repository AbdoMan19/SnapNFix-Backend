using FluentValidation;

namespace SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;

public class GetIncidentTrendsQueryValidator : AbstractValidator<GetIncidentTrendsQuery>
{
    public GetIncidentTrendsQueryValidator()
    {
        RuleFor(x => x.Interval)
            .IsInEnum()
            .WithMessage("Invalid statistics interval. Must be Monthly, Quarterly, or Yearly");
    }
}