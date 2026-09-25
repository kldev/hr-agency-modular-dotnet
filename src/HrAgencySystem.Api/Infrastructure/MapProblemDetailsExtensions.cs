using System.Diagnostics;
using HrAgencySystem.Api.Common.Errors;

namespace HrAgencySystem.Api.Infrastructure;

public static class MapProblemDetailsExtensions
{
    public static void AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            // The bare W3C trace id, not Activity.Id ("00-<trace>-<span>-01") nor TraceIdentifier:
            // this is the value the trace viewer searches by, so it can be pasted straight in.
            options.CustomizeProblemDetails = context =>
            {
                if (Activity.Current is { } activity)
                    context.ProblemDetails.Extensions["traceId"] = activity.TraceId.ToString();
            };
        });
    }
}
