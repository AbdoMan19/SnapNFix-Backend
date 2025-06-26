using FluentValidation;
using SnapNFix.Application.Resources;


namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandValidator : AbstractValidator<LoginWithPhoneOrEmailCommand>
{
    public LoginWithPhoneOrEmailCommandValidator()
    {
        
        RuleFor(f => f.EmailOrPhoneNumber)
            .NotEmpty()
            .WithMessage(Shared.EmailOrPhoneNumberRequired)
            .Matches(@"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)|(^01[0125][0-9]{8}$)")
            .WithMessage(Shared.InvalidEmailOrPhoneNumber);

        RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Shared.PasswordRequired)
                .MinimumLength(8).WithMessage(Shared.PasswordTooShort)
                .Matches("[A-Z]").WithMessage(Shared.PasswordMissingUpper)
                .Matches("[a-z]").WithMessage(Shared.PasswordMissingLower)
                .Matches("[0-9]").WithMessage(Shared.PasswordMissingNumber)
                .Matches("[^a-zA-Z0-9]").WithMessage(Shared.PasswordMissingSpecial);

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
            .NotEmpty()
            .WithMessage(Shared.FCMTokenRequired);

    }
    
}