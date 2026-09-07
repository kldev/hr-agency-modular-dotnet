using HrAgencySystem.Recruitment.Application.JobApplications.Queries;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeJobApplicationInfoQueryRepository : IJobApplicationInfoQueryRepository
{
    public Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, Guid organizationId, CancellationToken ct)
    {
        var result = new JobApplicationInfo(jobApplicationId, organizationId, Guid.NewGuid(), Guid.NewGuid());
        return Task.FromResult((JobApplicationInfo?)result);
    }
}