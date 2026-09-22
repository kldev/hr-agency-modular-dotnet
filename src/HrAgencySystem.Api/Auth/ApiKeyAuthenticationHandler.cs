using System.Security.Claims;
using System.Text.Encodings.Web;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Auth;

/// <summary>
/// A second scheme next to the bearer, not a policy on top of it: a program presenting a key has no
/// user, no organization and no role, so there is nothing for a bearer policy to look at. The key
/// travels in <see cref="HeaderName"/>, is hashed, and must match a key that was never revoked.
/// <para>
/// No header is <em>no result</em>, not a failure - the scheme is only ever asked by the internal
/// policy, and "nothing presented" is for that policy to turn into a 401.
/// </para>
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceApiKeyRepository keys
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";

    public const string HeaderName = "X-Api-Key";

    /// <summary>What an internal endpoint can ask about the caller: which key it used.</summary>
    public const string KeyIdClaim = "service_key_id";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var presented))
            return AuthenticateResult.NoResult();

        var value = presented.ToString();

        // Refused before the database: a user token pasted into the header is not worth a query.
        if (!ServiceApiKey.LooksLikeKey(value))
            return AuthenticateResult.Fail("Not a service API key.");

        var key = await keys.FindByHashAsync(SecureToken.Hash(value), Context.RequestAborted);

        if (key is null)
            return AuthenticateResult.Fail("Unknown service API key.");

        if (key.IsRevoked)
            return AuthenticateResult.Fail("This service API key has been revoked.");

        var identity = new ClaimsIdentity(
            [
                new Claim(KeyIdClaim, key.Id.ToString()),
                new Claim(ClaimTypes.Name, key.Name),
            ],
            SchemeName
        );

        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)
        );
    }
}
