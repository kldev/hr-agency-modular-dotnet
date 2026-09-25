using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

/// <summary>The same question <c>/healthz</c> asks, in the shape the health check service expects.</summary>
public sealed class FileServiceHealthCheck(FileServiceHealthProbe probe) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) =>
        await probe.CheckAsync(cancellationToken) switch
        {
            "UP" => HealthCheckResult.Healthy(),
            // Documents do not work, everything else does - which is what degraded means.
            "NOT_CONFIGURED" => HealthCheckResult.Degraded("FileService:BaseUrl is not set"),
            _ => HealthCheckResult.Degraded("File service does not answer"),
        };
}
