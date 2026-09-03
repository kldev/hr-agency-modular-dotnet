using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.JobDescription.Infrastructure.Query;

public sealed class JobDescriptionSnapshotRepository(IDocumentSession session) : IJobDescriptionSnapshotRepository
{
    public async Task<JobDescriptionSnapshot?> GetAsync(Guid jobDescriptionId, Guid organizationId,
        CancellationToken ct)
    {
        var result = await session.Query<JobDescriptionCreated>()
            .Where(z => z.JobDescriptionId == jobDescriptionId && z.OrganizationId == organizationId)
            .FirstOrDefaultAsync(ct);

        return result != null
            ? new JobDescriptionSnapshot(result.JobDescriptionId, result.Title, result.CompanyId)
            : null;
    }
}