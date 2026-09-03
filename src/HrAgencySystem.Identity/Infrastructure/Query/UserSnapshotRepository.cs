using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Query;

public sealed class UserSnapshotRepository(IDocumentSession session) : IUserSnapshotRepository
{
    public async Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return await session.Query<UserProjection>().Where(z => z.Id == userId)
            .Select(z => new UserSnapshot(z.Id, z.FirstName, z.LastName, z.Email)).FirstOrDefaultAsync(ct);

    }
}