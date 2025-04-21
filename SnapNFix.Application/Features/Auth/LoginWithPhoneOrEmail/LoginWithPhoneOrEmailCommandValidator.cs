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
        
        RuleFor(f => f.EmailOrPhoneNumber)
            .NotEmpty()
            .WithMessage("Email or Phone Number is required.")
            .Matches(@"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)|(^(\+20|0)?1[0125][0-9]{8}$)")
            .WithMessage("Input must be a valid email or Egyptian phone number.");
        
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MinimumLength(8).WithMessage("{PropertyName} must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("{PropertyName} must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("{PropertyName} must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("{PropertyName} must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("{PropertyName} must contain at least one special character");
        
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