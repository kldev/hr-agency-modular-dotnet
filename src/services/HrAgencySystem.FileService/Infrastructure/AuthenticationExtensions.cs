using System.Text;
using HrAgencySystem.FileService.Config;
using HrAgencySystem.FileService.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.FileService.Infrastructure;

public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        public void SetupServiceAuthorization(IConfiguration configuration)
        {
            var config =
                configuration.GetSection(FileServiceConfig.SectionName).Get<FileServiceConfig>()
                ?? throw new InvalidOperationException(
                    $"Configuration section '{FileServiceConfig.SectionName}' is missing."
                );

            if (string.IsNullOrWhiteSpace(config.Secret))
                throw new InvalidOperationException(
                    $"'{FileServiceConfig.SectionName}:Secret' is required - the service will not "
                        + "start without a signing key, because starting without one means accepting "
                        + "unsigned calls."
                );

            services
                .AddAuthorizationBuilder()
                .SetFallbackPolicy(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
                );

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Claims are read under the names the issuer wrote them: the inbound mapping
                    // would rename 'sub' and leave the binder looking for a claim that is no longer
                    // there.
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = FileServiceToken.Issuer,
                        ValidAudience = FileServiceToken.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config.Secret)
                        ),
                        ClockSkew = TimeSpan.FromSeconds(30),
                    };
                });
        }
    }
}
