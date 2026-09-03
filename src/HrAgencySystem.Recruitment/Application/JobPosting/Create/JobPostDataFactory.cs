using HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Create;

internal static class JobPostDataFactory
{
    internal static JdData Create(IJobPostData command)
    {
        var errors = new List<string>();

        var (title, error) = PostTitle.TryCreate(command.Title);
        if (error != null) {
            errors.Add(error);
        }
        
        var (summary, errorSummary) = LongText.TryCreate(command.Summary ??"");
        if (errorSummary != null) {
            errors.Add(errorSummary);
        }

        var (description, errorDescription) = LongText.TryCreate(command.Description, true, "Job description");

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
        
        var (languageCode, errorLanguageCode) = LanguageCode.TryCreate(command.LanguageCode);
        if (errorLanguageCode != null) {
            errors.Add(errorLanguageCode);
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
            countryCode!,
            languageCode!
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
        PostTitle Title,
        LongText Summary,
        LongText Description,
        JobLocation JobLocation,
        IReadOnlyList<EntryText> Responsibilities,
        IReadOnlyList<EntryText> Requirements,
        IReadOnlyList<EntryText> Skills,
        SalaryRange SalaryRange,
        CountryCode  CountryCode,
        LanguageCode LanguageCode
    );
}