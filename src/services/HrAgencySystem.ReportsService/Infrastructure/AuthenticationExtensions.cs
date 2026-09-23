using HrAgencySystem.ReportsService.Auth;
using HrAgencySystem.ReportsService.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.ReportsService.Infrastructure;

public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        public void SetupServiceAuthorization(IConfiguration configuration)
        {
            var config =
                configuration
                    .GetSection(ReportsServiceConfig.SectionName)
                    .Get<ReportsServiceConfig>()
                ?? throw new InvalidOperationException(
                    $"Configuration section '{ReportsServiceConfig.SectionName}' is missing."
                );

            if (string.IsNullOrWhiteSpace(config.Secret))
                throw new InvalidOperationException(
                    $"'{ReportsServiceConfig.SectionName}:Secret' is required - starting without a "
                        + "signing key would mean accepting unsigned calls."
                );

            var authorization = services
                .AddAuthorizationBuilder()
                .SetFallbackPolicy(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
                );

            ReportPolicies.Add(authorization);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Claims keep the names the issuer wrote: the inbound mapping would rename 'sub'.
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = ServiceTokenParameters.Create(
                        config.Secret
                    );
                });
        }
    }
}
