using System.Text.Json;
using HrAgencySystem.Observability.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrAgencySystem.Observability.AspNetCore;

public static class HealthEndpointsExtensions
{
    public const string Live = "/health/live";
    public const string Ready = "/health/ready";

    extension(IEndpointRouteBuilder app)
    {
        /// <summary>
        /// <c>/health/live</c> - the process answers; <c>/health/ready</c> - and so does everything it
        /// depends on. Anonymous and out of the OpenAPI document. The existing <c>/healthz</c> of each
        /// host is left alone, because compose and the job board read its shape.
        /// </summary>
        public IEndpointRouteBuilder MapHealthEndpoints()
        {
            app.MapHealthChecks(Live, new HealthCheckOptions { Predicate = _ => false })
                .AllowAnonymous()
                .ExcludeFromDescription();

            app.MapHealthChecks(
                    Ready,
                    new HealthCheckOptions
                    {
                        Predicate = check => check.Tags.Contains(HealthTags.Ready),
                        ResponseWriter = WriteReportAsync,
                    }
                )
                .AllowAnonymous()
                .ExcludeFromDescription();

            return app;
        }
    }

    /// <summary>
    /// Names, statuses and timings only. The descriptions and exceptions stay in the log: this
    /// endpoint is anonymous, and a connection error names hosts and ports.
    /// </summary>
    private static Task WriteReportAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var body = new
        {
            status = report.Status.ToString(),
            durationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
            }),
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(body, JsonSerializerOptions.Web),
            context.RequestAborted
        );
    }
}
