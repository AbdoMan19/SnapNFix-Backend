using FluentValidation;

namespace SnapNFix.Application.Features.CityChannel.Commands.UpdateCityChannelStatus
{
    public class UpdateCityChannelStatusCommandValidator : AbstractValidator<UpdateCityChannelStatusCommand>
    {
        public UpdateCityChannelStatusCommandValidator()
        {
            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("City ID is required");
            
            RuleFor(x => x.IsActive)
                .Must(value => value == true || value == false)
                .WithMessage("City channel status is required");
        }
    }
}