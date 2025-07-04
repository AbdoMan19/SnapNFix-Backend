using FluentValidation;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;

public class RequestForgetPasswordOtpCommandValidator : AbstractValidator<RequestForgetPasswordOtpCommand>
{
    public RequestForgetPasswordOtpCommandValidator()
    {
        RuleFor(f => f.EmailOrPhoneNumber)
            .NotEmpty()
            .WithMessage(Shared.EmailOrPhoneNumberRequired)
            .Matches(@"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)|(^01[0125][0-9]{8}$)")
            .WithMessage(Shared.InvalidEmailOrPhoneNumber);
    }
}