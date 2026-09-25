using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

/// <summary>The same question <c>/healthz</c> asks, in the shape the health check service expects.</summary>
public sealed class ReportsHealthCheck(ReportsHealthProbe probe) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) =>
        await probe.CheckAsync(cancellationToken) switch
        {
            "UP" => HealthCheckResult.Healthy(),
            // Reports and the dashboard do not work, everything else does.
            "NOT_CONFIGURED" => HealthCheckResult.Degraded("Reports:BaseUrl is not set"),
            _ => HealthCheckResult.Degraded("Reports service does not answer"),
        };
}
