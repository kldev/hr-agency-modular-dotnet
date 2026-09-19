using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Query;

public sealed class UserSnapshotRepository(IDocumentSession session) : IUserSnapshotRepository
{
    public async Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var result = await session
            .Query<UserProjection>()
            .Where(z => z.Id == userId)
            .Select(z => new UserSnapshot(z.Id, z.FirstName, z.LastName, z.Email))
            .FirstOrDefaultAsync(ct);

        if (result != null)
            return result;

        return await session
            .Query<UserCreated>()
            .Where(z => z.UserId == userId)
            .Select(z => new UserSnapshot(
                z.UserId,
                z.Contact.FirstName,
                z.Contact.LastName,
                z.Contact.Email
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserSnapshot?> GetUserAsync(
        Guid userId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<UserProjection>()
            .Where(z => z.Id == userId && z.OrganizationId == organizationId.Value)
            .Select(z => new UserSnapshot(z.Id, z.FirstName, z.LastName, z.Email))
            .FirstOrDefaultAsync(ct);

        if (result != null)
            return result;

        // The projection runs in the async daemon, so a user created moments ago may not be there
        // yet. The event carries the organization too, so the fallback stays scoped.
        return await session
            .Query<UserCreated>()
            .Where(z => z.UserId == userId && z.OrganizationId == organizationId.Value)
            .Select(z => new UserSnapshot(
                z.UserId,
                z.Contact.FirstName,
                z.Contact.LastName,
                z.Contact.Email
            ))
            .FirstOrDefaultAsync(ct);
    }
}
