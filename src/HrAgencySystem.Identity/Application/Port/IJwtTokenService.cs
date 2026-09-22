using HrAgencySystem.Identity.Projections;

namespace HrAgencySystem.Identity.Application.Port;

public interface IJwtTokenService
{
    AccessToken GenerateUserToken(UserProjection user);
    AccessToken GenerateOwnerToken(OwnerProjection owner);

    /// <summary>
    /// A token that is the target's in every way that authorization looks at - their id, their
    /// organization, their role - and additionally names the administrator standing in for them.
    /// </summary>
    AccessToken GenerateImpersonationToken(UserProjection user, Guid impersonatedBy);
}

/// The minted token together with the moment it stops being accepted, so a client can refresh
/// before a call fails rather than after.
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
