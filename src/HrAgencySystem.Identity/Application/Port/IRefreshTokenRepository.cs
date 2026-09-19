using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Port;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct);
    Task IssueAsync(RefreshToken token, CancellationToken ct);
    Task RotateAsync(RefreshToken spent, RefreshToken issued, CancellationToken ct);
    Task RevokeFamilyAsync(Guid familyId, CancellationToken ct);
    Task RevokeUserSessionsAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken ct
    );
}
