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
        var organizationId = OrganizationId.From(command.OrganizationId);
        var (title, summary, description,
            location, responsibilities,
            requirements, skills, salaryRange, countryCode) = JobDescriptionDataFactory.Create(command);

        if (!await checker.Exists(organizationId.Value, ct))
            throw new BusinessRuleException(OrganizationId.OrganizationCheckMessage);

        var recruiter = await userSnapshotRepository.GetUserAsync(command.RecruiterId, ct);
        if (recruiter == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);

        var createdBy = await userSnapshotRepository.GetUserAsync(command.CreatedBy, ct);
        if (createdBy == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        
        var company = await companySnapshotRepository.GetCompanyAsync(command.CompanyId, ct);
        if (company == null)
            throw new BusinessRuleException(ICompanySnapshotRepository.NotFoundMessage);
        
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
}