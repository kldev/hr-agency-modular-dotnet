using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Snapshots;

public sealed class UserSnapshotService(IDocumentSession session) : IUserSnapshotService
{
    public async Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return await session.Query<UserProjection>().Where(z => z.Id == userId)
            .Select(z => new UserSnapshot(z.Id, z.FirstName, z.LastName, z.Email)).FirstOrDefaultAsync(ct);

    }
}