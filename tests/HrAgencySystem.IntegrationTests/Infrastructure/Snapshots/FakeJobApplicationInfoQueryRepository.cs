using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeJobApplicationInfoQueryRepository : IJobApplicationInfoQueryRepository
{
    public Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, OrganizationId organizationId, CancellationToken ct)
    {
        var candidateInfo = new CandidateInfo(Guid.NewGuid(), "test@fake.com", "", "", "");
        var result = new JobApplicationInfo(jobApplicationId, organizationId.Value, Guid.NewGuid(), Guid.NewGuid(), candidateInfo);
        return Task.FromResult((JobApplicationInfo?)result);
    }
}