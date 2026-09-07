using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Infrastructure.Query;
using HrAgencySystem.Recruitment.Projections;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public class JobApplicationInfoQueryRepository(IQuerySession session) : IJobApplicationInfoQueryRepository
{
    public async Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, Guid organizationId, CancellationToken ct)
    {
        return await session.Query<JobApplicationProjection>()
            .WithOrganizationId(organizationId)
            .WithJobApplicationId(jobApplicationId)
            .Select(z => new JobApplicationInfo(z.Id, z.OrgId, z.CandidateId, z.CompanyId))
            .FirstOrDefaultAsync(ct);
    }
}