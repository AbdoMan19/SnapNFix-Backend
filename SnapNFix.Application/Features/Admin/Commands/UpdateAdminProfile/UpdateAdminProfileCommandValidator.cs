using FluentValidation;
using SnapNFix.Application.Resources;

public class UpdateAdminProfileCommandValidator : AbstractValidator<UpdateAdminProfileCommand>
{
    public UpdateAdminProfileCommandValidator()
    {
        // Validate FirstName only if provided
        RuleFor(x => x.FirstName)
            .Length(2, 50)
            .WithMessage(Shared.FirstNameLength)
            .Matches(@"^[a-zA-Z\u0600-\u06FF\s]+$")
            .WithMessage(Shared.FirstNameInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .Length(2, 50)
            .WithMessage(Shared.LastNameLength)
            .Matches(@"^[a-zA-Z\u0600-\u06FF\s]+$")
            .WithMessage(Shared.LastNameInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .WithMessage(Shared.InvalidPhoneNumber)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage(Shared.InvalidGender)
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.BirthDate)
            .Must(BeAValidAge)
            .WithMessage(Shared.InvalidAgeError)
            .Must(NotBeFutureDate)
            .WithMessage(Shared.FutureDateError)
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update")
            .OverridePropertyName("UpdateFields");
    }

    private static bool BeAValidAge(DateOnly? birthDate)
    {
        if (!birthDate.HasValue) return true;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDate.Value.Year;
        
        if (birthDate.Value > today.AddYears(-age))
            age--;

        return age >= 18 && age <= 120; 
    }

    private static bool NotBeFutureDate(DateOnly? birthDate)
    {
        if (!birthDate.HasValue) return true;

        var today = DateOnly.FromDateTime(DateTime.Today);
        return birthDate.Value <= today;
    }

    private static bool HaveAtLeastOneField(UpdateAdminProfileCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.FirstName) ||
               !string.IsNullOrWhiteSpace(command.LastName) ||
               !string.IsNullOrWhiteSpace(command.PhoneNumber) ||
               command.Gender.HasValue ||
               command.BirthDate.HasValue;
    }
}