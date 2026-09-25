using System.Reflection;
using HrAgencySystem.Observability.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace HrAgencySystem.Observability;

public static class ObservabilityExtensions
{
    /// <summary>Standard OpenTelemetry variable; set by compose, absent in tests and a bare run.</summary>
    public const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    /// <summary><c>text</c> (default, for a terminal) or <c>json</c> (for a container's log driver).</summary>
    public const string ConsoleFormatKey = "Observability:ConsoleFormat";

    private const string TextTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}";

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Serilog as the logging pipeline, OpenTelemetry as the way out. Serilog owns the levels
        /// (the <c>Serilog</c> section) and the console; every event that passes it is handed on to
        /// the OpenTelemetry logger provider, so logs leave through the same exporter and with the
        /// same resource as traces and metrics - one place decides where telemetry goes.
        /// </summary>
        public IHostApplicationBuilder AddObservability(string serviceName)
        {
            var exportsOtlp = !string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointKey]);

            builder.AddSerilogPipeline(serviceName, exportsOtlp);
            builder.AddOpenTelemetryPipeline(serviceName, exportsOtlp);

            // The host adds its own checks with AddHealthChecks(); this only makes sure every
            // report is also published as a metric, HTTP or not.
            builder.Services.AddHealthChecks();
            builder.Services.AddSingleton<IHealthCheckPublisher, HealthStatusMetrics>();

            return builder;
        }

        private void AddSerilogPipeline(string serviceName, bool exportsOtlp)
        {
            // The default console, debug and event source providers would otherwise receive every
            // event a second time through writeToProviders.
            builder.Logging.ClearProviders();

            if (exportsOtlp)
            {
                builder.Logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                });
            }

            var json = string.Equals(
                builder.Configuration[ConsoleFormatKey],
                "json",
                StringComparison.OrdinalIgnoreCase
            );

            builder.Services.AddSerilog(
                (services, logger) =>
                {
                    logger
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentName()
                        .Enrich.WithProperty("ServiceName", serviceName);

                    if (json)
                        logger.WriteTo.Console(new RenderedCompactJsonFormatter());
                    else
                        logger.WriteTo.Console(outputTemplate: TextTemplate);
                },
                writeToProviders: true
            );
        }

        private void AddOpenTelemetryPipeline(string serviceName, bool exportsOtlp)
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";

            var telemetry = builder
                .Services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                    resource
                        // The machine name is the container id, which tells replicas of a scaled
                        // service apart; a random id would change on every restart instead.
                        .AddService(
                            serviceName,
                            serviceVersion: version,
                            serviceInstanceId: Environment.MachineName
                        )
                        .AddAttributes(
                            [
                                new(
                                    "deployment.environment.name",
                                    builder.Environment.EnvironmentName
                                ),
                            ]
                        )
                )
                .WithTracing(tracing =>
                    tracing
                        .AddSource(
                            TelemetryNames.Application,
                            TelemetryNames.Wolverine,
                            TelemetryNames.Marten
                        )
                        .AddHttpClientInstrumentation()
                        .AddNpgsql()
                        .AddAWSInstrumentation()
                )
                .WithMetrics(metrics =>
                    metrics
                        .AddMeter(
                            TelemetryNames.Application,
                            TelemetryNames.WolverineMeters,
                            TelemetryNames.Marten,
                            TelemetryNames.Npgsql,
                            TelemetryNames.Runtime
                        )
                        .AddInstrumentation<ProcessLimitsMetrics>()
                        .AddHttpClientInstrumentation()
                        .AddAWSInstrumentation()
                        // A histogram sample recorded inside a sampled span keeps its trace id, so a
                        // dot on a latency chart in Grafana opens that very request in Rootprint.
                        .SetExemplarFilter(ExemplarFilterType.TraceBased)
                );

            // Tracing and metrics stay registered without an exporter: activities still exist, so a
            // problem details response carries a trace id either way. Only shipping them is optional,
            // and the OTLP variables (endpoint, protocol, headers) are read by the exporter itself.
            if (exportsOtlp)
                telemetry.UseOtlpExporter();
        }
    }
}
