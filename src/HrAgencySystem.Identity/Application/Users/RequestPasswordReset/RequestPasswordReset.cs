namespace HrAgencySystem.Identity.Application.Users.RequestPasswordReset;

/// <summary>
/// The organization is resolved the same way a login resolves it: by slug when the caller knows it,
/// by the email domain otherwise. <paramref name="PortalUrl"/> is the base the reset link is built
/// on and comes from the host's configuration, not from the caller.
/// </summary>
public sealed record RequestPasswordReset(string Email, string Slug, string PortalUrl);

/// <summary>
/// Deliberately says nothing about whether a mail went out - the answer to "does this address have
/// an account here" is not something an anonymous endpoint hands out.
/// </summary>
public sealed record RequestPasswordResetResult(DateTimeOffset RequestedAt);
