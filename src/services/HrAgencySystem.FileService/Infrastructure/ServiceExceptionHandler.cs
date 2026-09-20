using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.FileService.Infrastructure;

/// <summary>
/// Four outcomes, deliberately terse. This service talks to one caller - the API - so the body is
/// for a log, not for a person.
/// </summary>
internal sealed class ServiceExceptionHandler(ILogger<ServiceExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct
    )
    {
        var (status, title) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled failure on {Path}.", context.Request.Path);
        else
            logger.LogWarning("{Title} on {Path}: {Message}", title, context.Request.Path, exception.Message);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? null
                    : exception.Message,
            },
            ct
        );

        return true;
    }
}
