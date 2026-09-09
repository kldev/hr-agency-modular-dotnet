using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.JobDescription.Application.Create;

public static class CreateJobDescriptionHandler
{
    public static async Task<JobDescriptionCreated> Handle(
        CreateJobDescription command,
        IDocumentSession session,
        IClock clock,
        IJobDescriptionService service,
        CancellationToken ct)
    {
        var (title, summary, description,
            location, responsibilities,
            requirements, skills, salaryRange, countryCode) = JobDescriptionDataFactory.Create(command);

        await service.ValidateOrganization(command.OrganizationId, ct);

        var recruiter = await service.GetUserAsync(command.RecruiterId, ct);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var company = await service.GetCompanyAsync(command.CompanyId, ct);

        var jobDescriptionId = JobDescriptionId.New();
        var @event = new JobDescriptionCreated(
                jobDescriptionId.Value,
                command.OrganizationId,
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