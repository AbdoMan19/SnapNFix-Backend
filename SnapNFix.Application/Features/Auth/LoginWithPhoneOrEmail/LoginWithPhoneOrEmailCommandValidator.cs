using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandValidator : AbstractValidator<LoginWithPhoneOrEmailCommand>
{
    public LoginWithPhoneOrEmailCommandValidator(IUnitOfWork unitOfWork , UserManager<User> userManager)
    {
        RuleFor(x => new { x.Email, x.PhoneNumber })
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Either Email or Phone Number must be provided");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () => {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("A valid email address is required");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () => {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{8,15}$")
                .WithMessage("Phone number must be between 8 and 15 digits and may start with a '+' symbol");
        });

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long");
    }
    
}