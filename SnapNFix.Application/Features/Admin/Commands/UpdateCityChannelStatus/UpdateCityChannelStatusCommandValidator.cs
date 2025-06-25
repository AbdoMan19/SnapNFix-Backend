using FluentValidation;
using SnapNFix.Application.Features.Admin.Commands.UpdateCityChannelStatus;

namespace SnapNFix.Application.Features.Admin.Commands.UpdateCityChannelStatus
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