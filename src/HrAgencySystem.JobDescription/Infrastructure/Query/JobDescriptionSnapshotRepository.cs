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
            .Select(z => new JobDescriptionSnapshot(z.JobDescriptionId, z.Title, z.CompanyId))
            .FirstOrDefaultAsync(ct);


        if (result != null) return result;

        return await session.Query<JobDescriptionCreated>()
            .Where(z => z.JobDescriptionId == jobDescriptionId && z.OrganizationId == organizationId)
            .Select(z => new JobDescriptionSnapshot(z.JobDescriptionId, z.Title, z.CompanyId))
            .FirstOrDefaultAsync(ct);
    }
}