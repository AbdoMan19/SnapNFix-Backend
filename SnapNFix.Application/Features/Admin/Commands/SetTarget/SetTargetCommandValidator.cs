using FluentValidation;

public class SetTargetCommandValidator : AbstractValidator<SetTargetCommand>
    {
        public SetTargetCommandValidator()
        {
            RuleFor(x => x.TargetResolutionRate)
                .GreaterThan(0)
                .WithMessage("Target must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Target cannot exceed 100%");
        }
    }