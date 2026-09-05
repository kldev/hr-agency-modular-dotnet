using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class JobApplicationQueryRepository(IQuerySession session) : IJobApplicationQueryRepository
{
    public async Task<SliceResponse<JobApplicationProjection>> GetJobApplications(Guid organizationId, JobApplicationQuery query, CancellationToken ct)
    {
        return await session.Query<JobApplicationProjection>()
            .WithOrganizationId(organizationId)
            .WithCompanyId(query.CompanyId)
            .WithSources(query.Sources)
            .WithStatus(query.Status ?? [])
            .WithTags(query.Tags ?? [])
            .WithSearch(query.Search)
            .OrderByDescending(z=>z.CreatedAt)
            .ToSlice(query, ct);
    }

    public async Task<JobApplicationProjection?> GetJobApplication(Guid organizationId, Guid jobApplicationId,
        CancellationToken ct)
    {
        return await session.Query<JobApplicationProjection>()
            .WithOrganizationId(organizationId)
            .WithJobApplicationId(jobApplicationId)
            .FirstOrDefaultAsync(ct);
    }
}