using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.JobDescription.Application.Port;

public interface IJobDescriptionQueryRepository
{
    Task<SliceResponse<JobDescriptionProjection>> GetJobDescriptions(Guid organizationId, JobDescriptionQuery query, CancellationToken ct);
}

public sealed record JobDescriptionQuery(string Search, 
    Guid? CompanyId, 
    Guid? RecruiterId, 
    IReadOnlyList<JobDescriptionStatus> Statuses, 
    int Page, int PageSize) : IPagedQuery;