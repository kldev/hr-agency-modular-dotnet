using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrAgencySystem.Observability.Health;

/// <summary>
/// Publishes the latest health report as a gauge, one series per check. The two workers have no
/// HTTP port to probe, so this is how "the notification worker has lost RabbitMQ" reaches a
/// dashboard at all - and for the HTTP hosts it gives the same answer a history.
/// </summary>
public sealed class HealthStatusMetrics : IHealthCheckPublisher
{
    public const string MeterName = "HrAgencySystem.Health";

    private readonly ConcurrentDictionary<string, HealthStatus> _latest = new();

    public HealthStatusMetrics(IMeterFactory meters)
    {
        var meter = meters.Create(MeterName);
        meter.CreateObservableGauge(
            "hr.health.status",
            Observe,
            unit: "{status}",
            description: "1 when the check is healthy, 0.5 when degraded, 0 when unhealthy."
        );
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        foreach (var (name, entry) in report.Entries)
            _latest[name] = entry.Status;

        return Task.CompletedTask;
    }

    private IEnumerable<Measurement<double>> Observe() =>
        _latest.Select(pair => new Measurement<double>(
            pair.Value switch
            {
                HealthStatus.Healthy => 1,
                HealthStatus.Degraded => 0.5,
                _ => 0,
            },
            new KeyValuePair<string, object?>("check", pair.Key)
        ));
}
