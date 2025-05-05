using FluentValidation;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("Device Id is required.");
        RuleFor(x => x.DeviceName)
            .NotEmpty()
            .WithMessage("Device Name is required.");
        RuleFor(x => x.DeviceType)
            .NotEmpty()
            .WithMessage("Device Type is required.");
        RuleFor(x => x.Platform)
            .NotEmpty()
            .WithMessage("Platform is required.");
    }
    
}