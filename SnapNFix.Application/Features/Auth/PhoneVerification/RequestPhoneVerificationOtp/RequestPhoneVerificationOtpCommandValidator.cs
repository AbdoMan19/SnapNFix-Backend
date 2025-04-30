using FluentValidation;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Auth.PhoneVerification.RequestPhoneVerificationOtp;

public class RequestPhoneVerificationOtpCommandValidator : AbstractValidator<RequestPhoneVerificationOtpCommand>
{ 
    public RequestPhoneVerificationOtpCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+20|0)?1[0125][0-9]{8}$").WithMessage("{PropertyName} must be a valid Egyptian phone number")
            .Must((_, r) =>
            {
                var exist = unitOfWork.Repository<User>().ExistsByName(u => u.PhoneNumber == r && u.PhoneNumberConfirmed == true);
                return !exist;
            });
    }
}