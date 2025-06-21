using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Admin.Commands.AdminLogin;

public class AdminLoginCommandValidator : AbstractValidator<AdminLoginCommand>
{
    public AdminLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Shared.EmailRequired)
            .EmailAddress().WithMessage(Shared.InvalidEmailFormat);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Shared.PasswordRequired)
            .MinimumLength(8).WithMessage(Shared.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(Shared.PasswordMissingUpper)
            .Matches("[a-z]").WithMessage(Shared.PasswordMissingLower)
            .Matches("[0-9]").WithMessage(Shared.PasswordMissingNumber)
            .Matches("[^a-zA-Z0-9]").WithMessage(Shared.PasswordMissingSpecial);

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage(Shared.DeviceIdRequired);

        RuleFor(x => x.DeviceName)
            .NotEmpty().WithMessage(Shared.DeviceNameRequired);

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage(Shared.DeviceTypeRequired);

        RuleFor(x => x.Platform)
            .NotEmpty().WithMessage(Shared.PlatformRequired);
    }
}