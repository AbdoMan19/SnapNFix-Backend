using FluentValidation;
using SnapNFix.Application.Features.Users.Commands.UnsubscribeFromCityChannel;

namespace SnapNFix.Application.Features.Users.Commands.UnsubscribeFromCityChannel
{
    public class UnsubscribeFromCityChannelCommandValidator : AbstractValidator<UnsubscribeFromCityChannelCommand>
    {
        public UnsubscribeFromCityChannelCommandValidator()
        {
            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("City ID is required");
        }
    }
}