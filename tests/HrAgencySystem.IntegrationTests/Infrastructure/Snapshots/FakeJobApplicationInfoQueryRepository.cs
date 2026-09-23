using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

/// <summary>
/// Backed by the real repository when the application exists - a test that applied over HTTP
/// needs its interview to belong to that candidate - and made up otherwise, for the interview
/// tests that schedule against an application nobody created.
/// </summary>
public class FakeJobApplicationInfoQueryRepository(
    IQuerySession session,
    ILogger<JobApplicationInfoQueryRepository> logger
) : IJobApplicationInfoQueryRepository
{
    public async Task<JobApplicationInfo?> GetAsync(
        Guid jobApplicationId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var real = await new JobApplicationInfoQueryRepository(session, logger).GetAsync(
            jobApplicationId,
            organizationId,
            ct
        );
        if (real is not null)
            return real;

        var candidateInfo = new CandidateInfo(Guid.NewGuid(), "test@fake.com", "", "", "");
        return new JobApplicationInfo(
            jobApplicationId,
            organizationId.Value,
            Guid.NewGuid(),
            Guid.NewGuid(),
            candidateInfo,
            "Job Post Title",
            Guid.NewGuid()
        );
    }
}
