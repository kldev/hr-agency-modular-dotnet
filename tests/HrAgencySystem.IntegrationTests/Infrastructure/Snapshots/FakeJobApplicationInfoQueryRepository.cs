using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeJobApplicationInfoQueryRepository : IJobApplicationInfoQueryRepository
{
    public Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, OrganizationId organizationId, CancellationToken ct)
    {
        var result = new JobApplicationInfo(jobApplicationId, organizationId.Value, Guid.NewGuid(), Guid.NewGuid());
        return Task.FromResult((JobApplicationInfo?)result);
    }
}