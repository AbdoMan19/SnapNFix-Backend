using FluentValidation;

namespace SnapNFix.Application.Features.Users.Commands.SubscribeToCityChannel
{
    public class SubscribeToCityChannelCommandValidator : AbstractValidator<SubscribeToCityChannelCommand>
    {
        public SubscribeToCityChannelCommandValidator()
        {
            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("City ID is required");
        }
    }
}