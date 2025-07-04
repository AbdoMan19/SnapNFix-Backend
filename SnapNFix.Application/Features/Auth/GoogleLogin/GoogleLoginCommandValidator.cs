using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("Google ID token is required");

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
        RuleFor(x => x.FCMToken)
            .MaximumLength(200)
            .WithMessage("Shared.FCMTokenMaxLength");

        // FCMToken is optional, don't validate it as required
    }
}