namespace HrAgencySystem.Identity.Application.Users.Login;

public sealed record LoginUser(string Email, string Password, string Slug);

/// <summary>
/// What a client needs to hold a session: the bearer token it sends, the refresh token it exchanges
/// once that one runs out, and both expiry dates so it does not have to guess or duplicate the
/// configured lifetimes. Issued by logging in and by every refresh alike.
/// </summary>
public sealed record LoginUserResult(
    string Token,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt
);
