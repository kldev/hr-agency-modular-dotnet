using HrAgencySystem.Identity.Projections;

namespace HrAgencySystem.Identity.Application.Port;

public interface IJwtTokenService
{
    string GenerateUserToken(UserProjection user);
    string GenerateOwnerToken(OwnerProjection owner);
}