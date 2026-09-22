using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.Api.Auth;

/// <summary>
/// What the internal routes require, bound to the <see cref="ApiKeyAuthenticationHandler"/> scheme
/// alone. Both directions of the same rule matter: a user's token must not open these routes, and a
/// key must not open anything else - the fallback policy authenticates with the bearer only, so a
/// key is simply not seen there.
/// </summary>
public static class InternalApiPolicy
{
    public const string Name = "InternalApi";

    public static void AddInternalApiPolicy(this AuthorizationBuilder builder) =>
        builder.AddPolicy(
            Name,
            policy =>
                policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .RequireClaim(ApiKeyAuthenticationHandler.KeyIdClaim)
        );

    public static AuthenticationBuilder AddServiceApiKeys(this AuthenticationBuilder builder) =>
        builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationHandler.SchemeName,
            _ => { }
        );
}
