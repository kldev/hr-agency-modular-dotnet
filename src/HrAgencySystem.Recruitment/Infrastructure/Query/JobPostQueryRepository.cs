using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public class JobPostQueryRepository(IDocumentSession session) : IJobPostQueryRepository
{
    public async Task<SliceResponse<JobPostResponse>> GetJobDescriptions(Guid organizationId, JobPostQuery query, CancellationToken ct)
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
}