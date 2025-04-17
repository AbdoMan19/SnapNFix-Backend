using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandValidator : AbstractValidator<LoginWithPhoneOrEmailCommand>
{
    public LoginWithPhoneOrEmailCommandValidator()
    {
        RuleFor(x => x.EmailOrPhone)
            .NotEmpty().WithMessage("Either Email or Phone Number must be provided");
        /*RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 6 characters long");*/
    }
    
}