using System.Text;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.Api.Infrastructure;

public static class AuthenticationExtensions
{
    public static void SetupAppAuthorization(this IServiceCollection services, 
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                }
                else
                {
                    var cors = configuration["Cors"] ?? "";
                    var corsOrigins = cors.Split(",", StringSplitOptions.RemoveEmptyEntries);
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:4300")
                        .WithOrigins("http://localhost:8080")
                        .WithOrigins([.. corsOrigins]);

                }
            });
        });
        
        services.AddAuthorizationBuilder()
                    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        
        services.Configure<JwtConfig>(
            configuration.GetSection(JwtConfig.Section));
        
        var config = configuration
                         .GetSection(JwtConfig.Section)
                         .Get<JwtConfig>()
                     ?? throw new InvalidOperationException(
                         $"Configuration section '{JwtConfig.Section}' is missing.");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.Issuer,
                    ValidAudience = config.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config.SecretKey))
                };
            });
    }
}