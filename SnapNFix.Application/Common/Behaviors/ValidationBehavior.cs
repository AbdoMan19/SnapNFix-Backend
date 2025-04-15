using FluentValidation;
using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using System;
using System.Linq;

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
            var responseType = typeof(TResponse);
            var innerType = responseType.GetGenericArguments()[0];
            
            var genericMethod = typeof(ValidationBehavior<TRequest, TResponse>)
                .GetMethod(nameof(CreateFailureResponse), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .MakeGenericMethod(innerType);
            
            return (TResponse)genericMethod.Invoke(this, new object[] { failures });
        }
        
        return await next();
    }

    private GenericResponseModel<T> CreateFailureResponse<T>(IList<ErrorResponseModel> errors)
    {
        return GenericResponseModel<T>.Failure("Validation Failed", errors);
    }
}