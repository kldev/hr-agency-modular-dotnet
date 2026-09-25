using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace HrAgencySystem.EmailTemplates.Messaging;

/// <summary>
/// Opens and closes one connection to the broker. Wolverine does not expose its own connection, and
/// a check that borrowed it could not tell "the broker is down" from "our listener gave up".
/// </summary>
public sealed class RabbitMqHealthCheck(RabbitMqConfig config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var factory = new ConnectionFactory { Uri = new Uri(config.GetConnectionUri()) };

        try
        {
            await using var connection = await factory.CreateConnectionAsync(
                "health-check",
                cancellationToken
            );
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // ToString(), not the uri: the description ends up in a log, the password must not.
            return HealthCheckResult.Unhealthy($"RabbitMQ {config} unreachable", exception);
        }
    }
}

public static class RabbitMqHealthCheckExtensions
{
    extension(IHealthChecksBuilder builder)
    {
        public IHealthChecksBuilder AddRabbitMq(RabbitMqConfig config, IEnumerable<string> tags) =>
            builder.Add(
                new HealthCheckRegistration(
                    "rabbitmq",
                    _ => new RabbitMqHealthCheck(config),
                    HealthStatus.Unhealthy,
                    tags,
                    TimeSpan.FromSeconds(5)
                )
            );
    }
}
