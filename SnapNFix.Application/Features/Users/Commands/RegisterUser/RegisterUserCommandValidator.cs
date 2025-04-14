using FluentValidation;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone number is not valid")
                .MustAsync(async (phone, cancellation) => 
                    !_unitOfWork.Repository<User>().ExistsByName(u => u.PhoneNumber == phone)
                ).WithMessage("Phone number is already registered");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is not valid")
                .MustAsync(async (email, cancellation) => 
                    !_unitOfWork.Repository<User>().ExistsByName(u => u.Email == email)
                ).WithMessage("Email is already registered");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match")
                .When(x => !string.IsNullOrWhiteSpace(x.Password), ApplyConditionTo.CurrentValidator);
        }
    }
}