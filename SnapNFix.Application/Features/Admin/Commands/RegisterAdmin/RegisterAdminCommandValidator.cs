using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Admin.Commands.RegisterAdmin;

public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
{
    private readonly UserManager<User> _userManager;

    public RegisterAdminCommandValidator(UserManager<User> userManager)
    {
        _userManager = userManager;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z\u0600-\u06FF\s]+$")
            .WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z\u0600-\u06FF\s]+$")
            .WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email address is already registered");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required");

        RuleFor(x => x.DeviceName)
            .NotEmpty().WithMessage("Device name is required");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("Device type is required");

        RuleFor(x => x.Platform)
            .NotEmpty().WithMessage("Platform is required");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        return existingUser == null;
    }
}