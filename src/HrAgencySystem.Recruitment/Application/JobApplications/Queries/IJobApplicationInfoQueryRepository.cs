namespace HrAgencySystem.Recruitment.Application.JobApplications.Queries;

public interface IJobApplicationInfoQueryRepository
{
    Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, Guid organizationId, CancellationToken ct);   
}

public sealed record JobApplicationInfo(Guid JobApplicationId, Guid OrganizationId, Guid CandidateId, Guid CompanyId);