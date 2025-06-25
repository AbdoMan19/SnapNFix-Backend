using FluentValidation;
using SnapNFix.Application.Features.Users.Queries.GetUserSubscribedCitiesChannel;

namespace SnapNFix.Application.Features.Users.Queries.GetUserSubscribedCitiesChannel
{
    public class GetUserSubscribedCitiesChannelCommandValidator : AbstractValidator<GetUserSubscribedCitiesChannelCommand>
    {
        public GetUserSubscribedCitiesChannelCommandValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(50).WithMessage("Page size must not exceed 50 items");
        }
    }
}