using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.Impersonate;

/// <summary>
/// An administrator asking for a session as somebody else in their own organization. Not an
/// <c>IUpdateCommand</c>: nothing is written, no stream grows - it mints a token, the way logging in
/// does, and the only trace it leaves is a line in the log.
/// </summary>
public sealed record ImpersonateUser(
    Guid TargetUserId,
    OrganizationId OrganizationId,
    Guid AdminUserId
);

/// <summary>
/// Deliberately not <c>LoginUserResult</c>: there is no refresh token, and the shape is what says
/// so. Once these thirty minutes are gone there is nothing to exchange, so the session cannot
/// quietly outlive the reason it was opened.
/// </summary>
public sealed record ImpersonationResult(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid ImpersonatedUserId,
    Guid ImpersonatedBy
);
