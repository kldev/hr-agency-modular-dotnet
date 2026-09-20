using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.SharedKernel.Exception;
using JasperFx;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Common.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService service
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        switch (exception)
        {
            case OrganizationAccessDeniedException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status403Forbidden,
                    OrganizationAccessDeniedException.ProblemTitle,
                    exception.Message,
                    exception
                );
            case AuthorizationException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    "Authentication failed",
                    exception.Message,
                    exception
                );
            case ValidationException validationException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return await service.TryWriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = httpContext,
                        ProblemDetails = BadRequestDetails.CreateValidation(validationException),
                    }
                );

            case DocumentAlreadyExistsException document:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    MapDocumentErrors.Title(document.DocumentType.Name),
                    MapDocumentErrors.Details(document.DocumentType.Name),
                    exception
                );

            case InValidValueException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Invalid value",
                    exception.Message,
                    exception
                );

            case ArgumentException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Argument exception",
                    exception.Message,
                    exception
                );

            case BusinessRuleException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Business rule",
                    exception.Message,
                    exception
                );
            case NotFoundException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Not found",
                    exception.Message,
                    exception
                );
            case FileServiceException:
                // 503, not 500: the request was fine and the document is probably fine too - the
                // process that holds it is not answering. The person reading this needs to know it
                // is worth trying again, and that nobody has to hunt for a missing file.
                logger.LogError(
                    exception,
                    "File service failure. TraceId: {TraceId}",
                    httpContext.TraceIdentifier
                );
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status503ServiceUnavailable,
                    "Document storage unavailable",
                    exception.Message,
                    exception
                );
            case BadHttpRequestException:
                logger.LogError(
                    exception,
                    "BadHttpRequestException TraceId: {TraceId}",
                    httpContext.TraceIdentifier
                );
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    exception.Message,
                    exception
                );
            default:
                logger.LogError(
                    exception,
                    "Unhandled exception occurred. TraceId: {TraceId}",
                    httpContext.TraceIdentifier
                );

                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred.",
                    exception
                );
        }
    }

    private async Task<bool> WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        Exception exception
    )
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
                Detail = detail,
            },
        };

        await service.TryWriteAsync(problem);

        return true;
    }
}
