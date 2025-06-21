using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage(Shared.DeviceIdRequired);
        RuleFor(x => x.DeviceName)
            .NotEmpty()
            .WithMessage(Shared.DeviceNameRequired);
        RuleFor(x => x.DeviceType)
            .NotEmpty()
            .WithMessage(Shared.DeviceTypeRequired);
        RuleFor(x => x.Platform)
            .NotEmpty()
            .WithMessage(Shared.PlatformRequired);
    }
    
}