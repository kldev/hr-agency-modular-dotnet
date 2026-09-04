using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public class JobPostQueryRepository(IDocumentSession session) : IJobPostQueryRepository
{
    public async Task<SliceResponse<JobPostResponse>> GetJobPosts(Guid organizationId, JobPostQuery query, CancellationToken ct)
    {
        var result = await session.Query<JobPostProjection>()
            .WithOrganizationId(organizationId)
            .WithCompanyId(query.CompanyId)
            .WithRecruiterId(query.RecruiterId)
            .WithSearch(query.Search)
            .WithStatuses(query.Statuses)
            .WithLanguages(query.Languages)
            .ToSlice(query, ct);

        var response = 
            result.Content.Select(JobPostResponse.From).ToList();
        return new SliceResponse<JobPostResponse>(response, result.HasMore);
    }

    public async Task<JobPostInfo> GetJobPostInfo(Guid jobPostId, CancellationToken ct)
    {
        var result = await session.Query<JobPostProjection>().Where(z => z.Id == jobPostId).SingleOrDefaultAsync(ct);
        return result is null ? throw new NotFoundException("Job post not found") : new JobPostInfo(result.Id, result.OrgId, result.CompanyId);
    }

    public async Task<JobPostProjection?> GetJobPost(Guid organizationId, Guid jobPostId, CancellationToken ct)
    {
        return await session.Query<JobPostProjection>()
            .WithOrganizationId(organizationId)
            .WithPostId(jobPostId)
            .SingleOrDefaultAsync(ct);
    }
}