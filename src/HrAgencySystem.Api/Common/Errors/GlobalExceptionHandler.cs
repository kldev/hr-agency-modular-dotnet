using HrAgencySystem.SharedKernel.Exception;
using JasperFx;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Common.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService service)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return await service.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext, ProblemDetails = BadRequestDetails.CreateValidation(validationException)
                });

            case DocumentAlreadyExistsException document:
                return await WriteErrorAsync(httpContext, StatusCodes.Status409Conflict,
                    MapDocumentErrors.Title(document.DocumentType.Name),
                    MapDocumentErrors.Details(document.DocumentType.Name), exception);

            case ArgumentException:
                return await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, "Argument exception",
                    exception.Message, exception);

            case BusinessRuleException:
                return await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, "Business rule",
                    exception.Message, exception);
            case NotFoundException:
                return await WriteErrorAsync(httpContext, StatusCodes.Status404NotFound, "Not found", exception.Message,
                    exception);
            case BadHttpRequestException:
                return await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, "Invalid request", exception.Message,
                    exception);
            default:
                logger.LogError(
                    exception,
                    "Unhandled exception occurred. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);

                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred.",
                    exception);
        }
    }

    private async Task<bool> WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        Exception exception)
    {
        context.Response.StatusCode = statusCode;
        var problem = new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Title = title,
                Detail = detail
            }
        };

        await service.TryWriteAsync(problem);

        return true;
    }
}