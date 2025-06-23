using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Commands.RegisterAdmin;

public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegisterAdminCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(Shared.FirstNameRequired)
            .Length(2, 50).WithMessage(Shared.FirstNameLength)
            .Matches(@"^[a-zA-Z\u0600-\u06FF\s]+$")
            .WithMessage(Shared.FirstNameInvalid);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(Shared.LastNameRequired)
            .Length(2, 50).WithMessage(Shared.LastNameLength)
            .Matches(@"^[a-zA-Z\u0600-\u06FF\s]+$")
            .WithMessage(Shared.LastNameInvalid);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Shared.EmailRequired)
            .EmailAddress().WithMessage(Shared.InvalidEmailFormat)
            .MustAsync(BeUniqueEmail).WithMessage(Shared.EmailAlreadyExists);

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

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = _unitOfWork.Repository<User>().ExistsByName(u => u.Email == email);
        return existingUser == false;
    }
}