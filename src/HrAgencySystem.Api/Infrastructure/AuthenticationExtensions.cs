using System.Text;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.Api.Infrastructure;

public static class AuthenticationExtensions
{
    public static void SetupAppAuthorization(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                }
                else
                {
                    var cors = configuration["Cors"] ?? "";
                    var corsOrigins = cors.Split(",", StringSplitOptions.RemoveEmptyEntries);
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:4300")
                        .WithOrigins("http://localhost:8080")
                        .WithOrigins([.. corsOrigins]);
                }
            });
        });

        var authorization = services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        authorization.AddPayrollPolicy();
        authorization.AddRatesPolicy();
        authorization.AddAdminPolicy();
        authorization.AddFormsDesignPolicy();
        authorization.AddInternalApiPolicy();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer()
            // Asked only by the internal policy; the bearer stays the default for everything else.
            .AddServiceApiKeys();

        // From the validated options rather than read before Build(): a missing or short key stops
        // the host at startup with the reason, instead of throwing inside the first login.
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtConfig>>(
                (options, jwt) =>
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Value.Issuer,
                        ValidAudience = jwt.Value.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.Value.SecretKey)
                        ),
                    }
            );
    }
}
