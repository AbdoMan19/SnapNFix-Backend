using FluentValidation;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Matches(@"^(\+20|0)?1[0125][0-9]{8}$").WithMessage("{PropertyName} must be a valid Egyptian phone number")
                .Must((_, phone, context) =>
                {
                    var existingUser = unitOfWork.Repository<User>().ExistsByName(u => u.PhoneNumber == phone);
                    return !existingUser;
                }).WithMessage("{PropertyName} is already registered");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(8).WithMessage("{PropertyName} must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("{PropertyName} must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("{PropertyName} must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("{PropertyName} must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("{PropertyName} must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}