using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrAgencySystem.Files.Service;

/// <summary>
/// Whether the object store answers. Asks for a key that never exists: a "no" is a healthy store,
/// only a failure to get an answer is not - so the check needs no bucket content and no rights
/// beyond the ones the host already uses.
/// </summary>
public sealed class ObjectStorageHealthCheck(IObjectStorage storage, string bucket) : IHealthCheck
{
    private const string ProbeKey = "__health-probe__";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await storage.ExistsAsync(ProbeKey, bucket, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy($"Object storage bucket {bucket} unreachable", exception);
        }
    }
}

public static class ObjectStorageHealthCheckExtensions
{
    extension(IHealthChecksBuilder builder)
    {
        public IHealthChecksBuilder AddObjectStorage(string bucket, IEnumerable<string> tags) =>
            builder.Add(
                new HealthCheckRegistration(
                    "object-storage",
                    services => new ObjectStorageHealthCheck(
                        services.GetRequiredService<IObjectStorage>(),
                        bucket
                    ),
                    HealthStatus.Unhealthy,
                    tags,
                    TimeSpan.FromSeconds(5)
                )
            );
    }
}
