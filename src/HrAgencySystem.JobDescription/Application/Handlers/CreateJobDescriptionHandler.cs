using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.JobDescription.Application.Handlers;

public static class CreateJobDescriptionHandler
{
    public static async Task<JobDescriptionCreated> Handle(
        CreateJobDescription command,
        IDocumentSession session,
        IClock clock,
        IOrganizationChecker checker,
        IUserSnapshotRepository userSnapshotRepository,
        ICompanySnapshotRepository companySnapshotRepository,
        CancellationToken ct)
    {
        var (title, summary, description,
            location, responsibilities,
            requirements, skills, salaryRange, countryCode) = JobDescriptionDataFactory.Create(command);

        var organizationId = await ValidateOrganization(command, checker, ct);

        var recruiter = await GetRecruiter(command, userSnapshotRepository, ct);

        var createdBy = await GetCreatedBy(command, userSnapshotRepository, ct);

        var company = await GetCompany(command, companySnapshotRepository, ct);

        var jobDescriptionId = JobDescriptionId.New();
        var @event = new JobDescriptionCreated(
                jobDescriptionId.Value,
                organizationId.Value,
                command.CompanyId,
                title.Value,
                summary.Value,
                description.Value,
                [.. responsibilities.Select(z => z.Value)],
                [.. requirements.Select(x => x.Value)],
                [.. skills.Select(x => x.Value)],
                location.Value,
                countryCode.Value,
                command.EmploymentType,
                command.WorkMode,
                salaryRange.Currency,
                salaryRange.Min,
                salaryRange.Max,
                recruiter!,
                createdBy,
                company,
                clock.UtcNow);

        session.Events.StartStream<Domain.JobDescription>(jobDescriptionId.Value, @event);

        return @event;
    }

    private static async Task<CompanySnapshot> GetCompany(CreateJobDescription command, ICompanySnapshotRepository companySnapshotRepository,
        CancellationToken ct)
    {
        var company = await companySnapshotRepository.GetCompanyAsync(command.CompanyId, ct);
        return company ?? throw new BusinessRuleException(ICompanySnapshotRepository.NotFoundMessage);
    }

    private static async Task<UserSnapshot> GetCreatedBy(CreateJobDescription command, IUserSnapshotRepository userSnapshotRepository,
        CancellationToken ct)
    {
        var createdBy = await userSnapshotRepository.GetUserAsync(command.CreatedBy, ct);
        return createdBy ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }

    private static async Task<UserSnapshot> GetRecruiter(CreateJobDescription command, IUserSnapshotRepository userSnapshotRepository,
        CancellationToken ct)
    {
        var recruiter = await userSnapshotRepository.GetUserAsync(command.RecruiterId, ct);
        return recruiter ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }

    private static async Task<OrganizationId> ValidateOrganization(CreateJobDescription command, IOrganizationChecker checker,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        if (!await checker.Exists(organizationId.Value, ct))
            throw new BusinessRuleException(OrganizationId.OrganizationCheckMessage);
        return organizationId;
    }
}