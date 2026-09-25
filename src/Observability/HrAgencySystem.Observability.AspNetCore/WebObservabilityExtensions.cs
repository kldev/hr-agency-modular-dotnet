using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace HrAgencySystem.Observability.AspNetCore;

public static class WebObservabilityExtensions
{
    /// <summary>
    /// Paths nobody wants a span or an information line for: probes every few seconds, the API
    /// docs and their assets.
    /// </summary>
    private static readonly string[] QuietPrefixes =
    [
        "/healthz",
        "/health",
        "/docs",
        "/openapi",
        "/scalar",
        "/_framework",
        "/favicon",
    ];

    extension(IHostApplicationBuilder builder)
    {
        /// <summary><see cref="ObservabilityExtensions.AddObservability"/> plus the HTTP server side.</summary>
        public IHostApplicationBuilder AddWebObservability(string serviceName)
        {
            builder.AddObservability(serviceName);

            builder
                .Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                    tracing.AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context => !IsQuiet(context.Request.Path);
                    })
                )
                .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation());

            return builder;
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// One line per request instead of the several <c>Microsoft.AspNetCore</c> writes, carrying
        /// the route template next to the concrete path so requests can be grouped by endpoint.
        /// Belongs before the endpoints so it times the whole pipeline.
        /// </summary>
        public IApplicationBuilder UseRequestLogging() =>
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "HTTP {RequestMethod:l} {RequestPath:l} responded {StatusCode} in {Elapsed:0.0} ms";
                options.GetLevel = (context, _, exception) =>
                    exception is not null || context.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                    : IsQuiet(context.Request.Path) ? LogEventLevel.Verbose
                    : LogEventLevel.Information;
                options.EnrichDiagnosticContext = (diagnostics, context) =>
                {
                    if (context.GetEndpoint() is RouteEndpoint endpoint)
                        diagnostics.Set("Route", endpoint.RoutePattern.RawText);
                };
            });
    }

    private static bool IsQuiet(PathString path) =>
        QuietPrefixes.Any(prefix => path.StartsWithSegments(prefix));
}
