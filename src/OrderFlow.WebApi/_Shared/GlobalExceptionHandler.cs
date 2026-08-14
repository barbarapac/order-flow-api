using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Domain._Shared;

namespace OrderFlow.WebApi._Shared;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Requisição cancelada pelo cliente: {TraceId}", httpContext.TraceIdentifier);
            return true;
        }

        var (statusCode, title, extensions) = Describe(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = statusCode;

        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception   = exception,
            ProblemDetails = new ProblemDetails
            {
                Status     = statusCode,
                Title      = title,
                Extensions = extensions!
            }
        });
    }

    private static (int StatusCode, string Title, Dictionary<string, object?> Extensions) Describe(Exception exception) =>
        exception switch
        {
            BadHttpRequestException badHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Failed to read the request body.",
                new Dictionary<string, object?> { ["detail"] = badHttpRequestException.Message }),

            FluentValidation.ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                new Dictionary<string, object?>
                {
                    ["errors"] = validationException.Errors
                        .GroupBy(failure => failure.PropertyName)
                        .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray())
                }),

            DomainException appException => (
                appException.Type.ToStatusCode(),
                appException.Message,
                new Dictionary<string, object?> { ["errorCode"] = appException.Code }),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                new Dictionary<string, object?>())
        };
}
