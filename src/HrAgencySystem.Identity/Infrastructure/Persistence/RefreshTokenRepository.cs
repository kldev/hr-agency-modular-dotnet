using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed class RefreshTokenRepository(IDocumentSession session) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct)
    {
        return await session
            .Query<RefreshToken>()
            .Where(z => z.TokenHash == tokenHash)
            .SingleOrDefaultAsync(ct);
    }

    public Task IssueAsync(RefreshToken token, CancellationToken ct)
    {
        session.Insert(token);

        return Task.CompletedTask;
    }

    public Task RotateAsync(RefreshToken spent, RefreshToken issued, CancellationToken ct)
    {
        // One transaction: there is never a moment where the old token is spent and the new one
        // does not exist yet.
        session.Update(spent);
        session.Insert(issued);

        return Task.CompletedTask;
    }

    public Task RevokeFamilyAsync(Guid familyId, CancellationToken ct)
    {
        session.DeleteWhere<RefreshToken>(z => z.FamilyId == familyId);

        return Task.CompletedTask;
    }

    public Task RevokeUserSessionsAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken ct
    )
    {
        session.DeleteWhere<RefreshToken>(z =>
            z.OrganizationId == organizationId.Value && z.UserId == userId.Value
        );

        return Task.CompletedTask;
    }
}
