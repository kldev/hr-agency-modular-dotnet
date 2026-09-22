using HrAgencySystem.Web.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Web.Common.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService service,
    IHostEnvironment environment
) : IExceptionHandler
{
    /// <summary>
    /// Whether the person reading the response is the person who can fix it. Locally a 500 says
    /// what actually went wrong; anywhere else it says nothing, because an exception message names
    /// internal types and a stack trace names the source tree.
    /// </summary>
    private bool ShowsInternals =>
        environment.IsDevelopment() || environment.EnvironmentName == "docker";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        // Domain exceptions stopped being possible here when the database went: the API raises
        // them and answers with problem details, which the client turns into results.
        switch (exception)
        {
            case JobBoardUnavailableException:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status503ServiceUnavailable,
                    "Service unavailable",
                    "The job board cannot reach its data right now. Try again in a moment.",
                    exception
                );
            case BadHttpRequestException:
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
                    ShowsInternals ? exception.Message : "An unexpected error occurred.",
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
        var details = new ProblemDetails
        {
            Type = exception.GetType().Name,
            Title = title,
            Detail = detail,
        };

        if (ShowsInternals)
        {
            details.Extensions["exception"] = exception.ToString();

            if (exception.InnerException is not null)
                details.Extensions["innerException"] = exception.InnerException.ToString();
        }

        var problem = new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = details,
        };

        await service.TryWriteAsync(problem);

        return true;
    }
}
