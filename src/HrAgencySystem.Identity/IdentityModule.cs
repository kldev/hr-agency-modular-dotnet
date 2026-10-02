using System.Text;
using HrAgencySystem.Identity.Infrastructure.Configuration;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Sagas;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace HrAgencySystem.Identity;

public static class IdentityModule
{
    public static void AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddIdentityServices();

        services
            .AddOptions<IdentityConfig>()
            .Bind(configuration.GetSection(IdentityConfig.Section))
            .Validate(
                config => config.PasswordResetExpiresInMinutes > 0,
                $"{IdentityConfig.Section}:PasswordResetExpiresInMinutes must be positive."
            )
            .ValidateOnStart();

        services
            .AddOptions<JwtConfig>()
            .Bind(configuration.GetSection(JwtConfig.Section))
            .Validate(
                config =>
                    !string.IsNullOrWhiteSpace(config.Issuer)
                    && !string.IsNullOrWhiteSpace(config.Audience),
                $"{JwtConfig.Section}:Issuer and {JwtConfig.Section}:Audience are required."
            )
            .Validate(
                config => Encoding.UTF8.GetByteCount(config.SecretKey) >= JwtConfig.MinSecretBytes,
                $"{JwtConfig.Section}:SecretKey must be at least {JwtConfig.MinSecretBytes} bytes - "
                    + "it signs HMAC-SHA256 tokens."
            )
            .Validate(
                config =>
                    config
                        is {
                            ExpiresInHours: > 0,
                            RefreshTokenExpiresInDays: > 0,
                            ImpersonationExpiresInMinutes: > 0,
                        },
                $"{JwtConfig.Section} token lifetimes must be positive."
            )
            .ValidateOnStart();
    }

    /// <summary>
    /// The password reset saga waits out its window as a scheduled message. A local queue is an
    /// in-memory queue by default, so without a durable inbox a restart of the API would drop the
    /// timeout and leave the saga behind forever.
    /// </summary>
    public static void ConfigureWolverine(WolverineOptions options)
    {
        options.LocalQueueFor<StartPasswordReset>().UseDurableInbox();
        options.LocalQueueFor<PasswordResetExpired>().UseDurableInbox();

        // Same reasoning for team membership: losing one of these silently leaves UserProjection.Team
        // disagreeing with the roster for good. UserTeamChanged needs no entry — it is invoked inline
        // from this queue's handler, so it is covered by that envelope's retries.
        options.LocalQueueFor<TeamMembershipChanged>().UseDurableInbox();
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
