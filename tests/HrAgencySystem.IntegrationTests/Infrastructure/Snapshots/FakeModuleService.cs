using HrAgencySystem.Company.Services;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeModuleService(
    ITeamSnapshotRepository teams,
    IJobApplicationInfoQueryRepository applications
) : IRecruitmentService, ICompanyService, IJobDescriptionService, IIdentityService
{
    public Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var result = new UserSnapshot(userId, "Test", "User", "test.user@test.io");

        return Task.FromResult(result);
    }

    public Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var suffix = companyId.ToString().Substring(4);
        var result = new CompanySnapshot(companyId, "Company  " + suffix, "TXT 101-200" + suffix);

        return Task.FromResult(result);
    }

    public Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task<OrganizationInfo> GetOrganization(
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var info = new OrganizationInfo(organizationId.Value, "hr-test", "Test");
        return Task.FromResult(info);
    }

    /// <summary>
    /// Real when the application was created over HTTP, made up otherwise - see
    /// <see cref="FakeJobApplicationInfoQueryRepository"/>.
    /// </summary>
    public async Task<JobApplicationInfo> GetApplicationAsync(
        Guid jobApplicationId,
        Guid organizationId,
        CancellationToken ct
    )
    {
        var application = await applications.GetAsync(
            jobApplicationId,
            OrganizationId.From(organizationId),
            ct
        );

        return application!;
    }

    /// <summary>
    /// Backed by the real thing. The other fakes stand in for data these tests
    /// never create; a team, by contrast, is created over HTTP by the test itself, so faking the
    /// lookup would hide both the happy path and the "no such team" rule.
    /// </summary>
    public async Task<TeamSnapshot> GetTeamAsync(
        Guid teamId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var team = await teams.GetTeamAsync(teamId, organizationId, ct);

        return team ?? throw new BusinessRuleException(ITeamSnapshotRepository.NotFoundMessage);
    }

    public Task<string> GetOrganizationSlug(OrganizationId organizationId, CancellationToken ct)
    {
        return Task.FromResult("Slug");
    }

    public void ValidateAggregateUpdate(IOrganizationDomain? aggregate, Guid commandOrganizationId)
    {
        if (aggregate == null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }

    public Task AppendApplicationNoteToStream(
        JobApplicationId jobApplicationId,
        OrganizationId organizationId,
        string note,
        UserSnapshot user,
        CancellationToken ct
    )
    {
        return Task.CompletedTask;
    }
}
