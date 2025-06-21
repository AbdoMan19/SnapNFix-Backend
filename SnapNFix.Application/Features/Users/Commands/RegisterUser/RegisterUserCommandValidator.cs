using FluentValidation;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(Shared.FirstNameRequired)
                .MaximumLength(50).WithMessage(Shared.FirstNameLength);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(Shared.LastNameRequired)
                .MaximumLength(50).WithMessage(Shared.LastNameLength);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Shared.PasswordRequired)
                .MinimumLength(8).WithMessage(Shared.PasswordTooShort)
                .Matches("[A-Z]").WithMessage(Shared.PasswordMissingUpper)
                .Matches("[a-z]").WithMessage(Shared.PasswordMissingLower)
                .Matches("[0-9]").WithMessage(Shared.PasswordMissingNumber)
                .Matches("[^a-zA-Z0-9]").WithMessage(Shared.PasswordMissingSpecial);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(Shared.ConfirmPasswordRequired)
                .Equal(x => x.Password).WithMessage(Shared.PasswordsDoNotMatch);

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
}