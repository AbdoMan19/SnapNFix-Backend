using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Utilities;

namespace SnapNFix.Api.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment webHostEnvironment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            "Exception Message: {Message}, StackTrace: {StackTrace}",
            exception.Message,
            exception.StackTrace ?? string.Empty);

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var errorMessage = EnvironmentsChecker.IsInDevelopmentMode(webHostEnvironment)
            ? $"Exception Message: {exception.Message}, \n " +
              $"Inner Exception Message: {exception.InnerException?.Message}, \n" +
              $"Stack Trace: {exception.StackTrace}"
            : ".Technical Failure Occurrs";

        var exceptionFailure = GenericResponseModel<string>.Failure(exception.Message, new List<ErrorResponseModel>
        {
            new()
            {
                Message = errorMessage,
                PropertyName = "Exception"
            }
        });

        await httpContext.Response.WriteAsJsonAsync(exceptionFailure);

        return true;
    }
}