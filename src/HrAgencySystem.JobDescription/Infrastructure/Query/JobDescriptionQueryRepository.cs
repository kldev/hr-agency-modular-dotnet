using HrAgencySystem.JobDescription.Application.Port;
using HrAgencySystem.JobDescription.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.JobDescription.Infrastructure.Query;

public class JobDescriptionQueryRepository(IDocumentSession session) : IJobDescriptionQueryRepository
{
    public async Task<SliceResponse<JobDescriptionProjection>> GetJobDescriptions(Guid organizationId, JobDescriptionQuery query, CancellationToken ct)
    {
        return await session.Query<JobDescriptionProjection>()
            .WithOrganizationId(organizationId)
            .WithCompanyId(query.CompanyId)
            .WithRecruiterId(query.RecruiterId)
            .WithSearch(query.Search)
            .WithStatuses(query.Statuses)
            .ToSlice(query, ct);
    }

    public async Task<JobDescriptionProjection?> GetJobDescription(Guid organizationId, Guid jobDescriptionId, CancellationToken ct)
    {
        return await session.Query<JobDescriptionProjection>()
            .WithOrganizationId(organizationId)
            .WithJobDescriptionId(jobDescriptionId)
            .SingleOrDefaultAsync(ct);
    }
}