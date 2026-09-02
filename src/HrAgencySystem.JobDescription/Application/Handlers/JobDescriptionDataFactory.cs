using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.JobDescription.Application.Handlers;

internal static class JobDescriptionDataFactory
{
    internal static JdData Create(IJobDescription command)
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
    
    internal sealed record JdData(
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