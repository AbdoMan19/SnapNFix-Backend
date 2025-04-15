using FluentValidation;
using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : BaseResponseModel

{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {

        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(failure => ErrorResponseModel.Create(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToList();

        if (failures.Any())
        {
            return GenericResponseModel<bool>.Failure("Validation Failed", failures) as TResponse;
            
        }
        return await next();
    }
}