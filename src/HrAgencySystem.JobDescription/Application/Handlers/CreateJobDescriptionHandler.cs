using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.JobDescription.Application.Handlers;

public static class CreateJobDescriptionHandler
{
    public static async Task<JobDescriptionCreated> Handle(
        CreateJobDescription command,
        IDocumentSession session,
        IClock clock,
        IOrganizationChecker checker,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var (title, summary, description,
            location, responsibilities,
            requirements, skills, salaryRange, countryCode) = CreateValueObjects(command);

        if (!await checker.Exists(organizationId.Value, ct))
            throw new BusinessRuleException(OrganizationId.OrganizationCheckMessage);

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
                salaryRange,
                command.RecruiterId,
                clock.UtcNow);

        session.Events.StartStream<Domain.JobDescription>(jobDescriptionId.Value, @event);

        return @event;
    }

    private static JdData CreateValueObjects(CreateJobDescription command)
    {
        var errors = new List<string>();

        var (title, error) = JobTitle.TryCreate(command.Title);
        if (error != null) {
            errors.Add(error);
        }
        
        var (summary, errorSummary) = JobSummary.TryCreate(command.Summary);
        if (errorSummary != null) {
            errors.Add(errorSummary);
        }

        var (description, errorDescription) = JobDescriptionText.TryCreate(command.Description);

        if (errorDescription != null) {
            errors.Add(errorDescription);
        }
        
        var (location, errorLocation) = JobLocation.TryCreate(command.Location);
        if (errorLocation != null) {
            errors.Add(errorLocation);
        }

        var (responsibilities, errorsResponsibilities) = TryCreateEntries(command.Responsibilities);
        var (requirements, errorsRequirements) = TryCreateEntries(command.Requirements);
        var (skills, errorsSkills) = TryCreateEntries(command.Skills);
        errors.AddRange(errorsResponsibilities);
        errors.AddRange(errorsRequirements);
        errors.AddRange(errorsSkills);

        var (salary, errorSalary) = SalaryRange.TryCreate(command.SalaryMin,
            command.SalaryMax, command.CurrencyCode);

        if (errorSalary != null) {
            errors.Add(errorSalary);
        }
        
        var (countryCode, errorCountryCode) = CountryCode.TryCreate(command.CountryCode);
        if (errorCountryCode != null) {
            errors.Add(errorCountryCode);
        }
        
        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new JdData(
            title!,
            summary!,
            description!,
            location!,
            responsibilities,
            requirements,
            skills,
            salary!,
            countryCode!
        );
    }

    private static (List<EntryText> entries, List<string> errors) TryCreateEntries(IReadOnlyList<string> input)
    {
        var entries = new List<EntryText>();
        var errors = new List<string>();

        foreach (var item in input)
        {
            var (entry, error) = EntryText.TryCreate(item);
            if (error != null) {
                errors.Add(error);
                continue;
            }
            entries.Add(entry!);
        }
        
        return (entries, errors);
    }

    private sealed record JdData(
        JobTitle Title,
        JobSummary Summary,
        JobDescriptionText Description,
        JobLocation JobLocation,
        IReadOnlyList<EntryText> Responsibilities,
        IReadOnlyList<EntryText> Requirements,
        IReadOnlyList<EntryText> Skills,
        SalaryRange SalaryRange,
        CountryCode  CountryCode
    );
}