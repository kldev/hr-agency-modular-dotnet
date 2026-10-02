using JetBrains.Annotations;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public sealed class JwtConfig
{
    public const string Section = "Jwt";

    /// <summary>
    /// HMAC-SHA256 needs a key of at least 256 bits. A shorter one is refused by the token handler -
    /// on the first login rather than at startup, unless the options are validated.
    /// </summary>
    public const int MinSecretBytes = 32;

    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string SecretKey { get; init; } = "";

    /// How long an access token stays valid. Short on purpose: it cannot be revoked, so the refresh
    /// token is what carries the session and this is only the window an intercepted token buys.
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public int ExpiresInHours { get; init; } = 6;

    /// How long a refresh token stays valid, counted from the login. Rotation inherits the date, so
    /// this is the whole session length - after it the user signs in again.
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public int RefreshTokenExpiresInDays { get; init; } = 30;

    /// How long a token issued by signing in as somebody else lasts. Much shorter than a login, and
    /// there is no refresh token to go with it, so the session ends on its own rather than because
    /// anybody remembered to end it.
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public int ImpersonationExpiresInMinutes { get; init; } = 30;
}
