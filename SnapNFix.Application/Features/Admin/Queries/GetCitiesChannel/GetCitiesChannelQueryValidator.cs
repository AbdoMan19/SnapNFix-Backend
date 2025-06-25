using FluentValidation;
using SnapNFix.Application.Features.Admin.Queries.GetCitiesChannel;

namespace SnapNFix.Application.Features.Admin.Queries.GetCitiesChannel
{
    public class GetCitiesChannelQueryValidator : AbstractValidator<GetCitiesChannelQuery>
    {
        public GetCitiesChannelQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100 items");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}