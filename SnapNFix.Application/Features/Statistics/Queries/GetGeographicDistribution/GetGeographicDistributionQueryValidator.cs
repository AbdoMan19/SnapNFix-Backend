using FluentValidation;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Statistics.Queries.GetGeographicDistribution;

public class GetGeographicDistributionQueryValidator : AbstractValidator<GetGeographicDistributionQuery>
{
    public GetGeographicDistributionQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithMessage("Limit must be greater than zero")
            .LessThanOrEqualTo(100)
            .WithMessage("Limit must not exceed 100 for performance reasons");
    }
}