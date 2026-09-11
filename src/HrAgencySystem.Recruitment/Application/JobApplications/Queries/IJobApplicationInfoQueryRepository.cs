using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Queries;

public interface IJobApplicationInfoQueryRepository
{
    Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, OrganizationId organizationId, CancellationToken ct);   
}

public sealed record JobApplicationInfo(Guid JobApplicationId, Guid OrganizationId, Guid CandidateId, Guid CompanyId, CandidateInfo Candidate);

