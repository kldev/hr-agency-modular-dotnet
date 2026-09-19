using HrAgencySystem.Identity.Projections;

namespace HrAgencySystem.Identity.Application.Port;

public interface IJwtTokenService
{
    AccessToken GenerateUserToken(UserProjection user);
    AccessToken GenerateOwnerToken(OwnerProjection owner);
}

/// The minted token together with the moment it stops being accepted, so a client can refresh
/// before a call fails rather than after.
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
