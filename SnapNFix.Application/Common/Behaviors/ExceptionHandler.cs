using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Common.Behaviors;

public sealed class ExceptionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, GenericResponseModel<TResponse>>
    where TRequest : IRequest<GenericResponseModel<TResponse>>
{
    private readonly ILogger<ExceptionBehavior<TRequest, TResponse>> _logger;

    public ExceptionBehavior(ILogger<ExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<GenericResponseModel<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<GenericResponseModel<TResponse>> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {RequestName}", typeof(TRequest).Name);
            return GenericResponseModel<TResponse>.Failure(
                "An unexpected error occurred",
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("server.error", ex.Message)
                });
        }
    }
}