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
        RuleFor(x => x.PhoneOrEmail)
            .NotEmpty();
        RuleFor(x => x.Password)
            .NotEmpty();
    }
    
}