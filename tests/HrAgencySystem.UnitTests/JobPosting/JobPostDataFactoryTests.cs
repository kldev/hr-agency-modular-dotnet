using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.Recruitment.Application.JobPosting.Update;
using HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.JobPosting;

public sealed class JobPostDataFactoryTests
{
    [Fact]
    public void Create_should_return_job_post_data_when_all_data_is_valid()
    {
        var command = new UpdateJobPost(
            JobPostId: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            Title: "Senior .NET Developer",
            Summary: "Senior .NET developer position.",
            Description: "Development of recruitment platform.",
            Responsibilities:
            [
                "Develop backend services",
                "Review code",
                "Design solutions"
            ],
            Requirements:
            [
                "C#",
                ".NET",
                "PostgreSQL"
            ],
            Skills:
            [
                "Clean Architecture",
                "DDD",
                "Marten"
            ],
            Location: "Opole",
            CountryCode: "pl",
            LanguageCode: "en",
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Hybrid,
            CurrencyCode: CurrencyCode.PLN,
            SalaryMin: 15000m,
            SalaryMax: 22000m,
            ModifiedBy: Guid.NewGuid());

        var result = JobPostDataFactory.Create(command);

        Assert.Equal("Senior .NET Developer", result.Title.Value);
        Assert.Equal(
            "Senior .NET developer position.",
            result.Summary.Value);

        Assert.Equal(
            "Development of recruitment platform.",
            result.Description.Value);

        Assert.Equal(
            "Opole",
            result.JobLocation.Value);

        Assert.Equal(
            ["Develop backend services", "Review code", "Design solutions"],
            result.Responsibilities.Select(x => x.Value));

        Assert.Equal(
            ["C#", ".NET", "PostgreSQL"],
            result.Requirements.Select(x => x.Value));

        Assert.Equal(
            ["Clean Architecture", "DDD", "Marten"],
            result.Skills.Select(x => x.Value));

        Assert.Equal(
            "PL",
            result.CountryCode.Value);

        Assert.Equal(
            "EN",
            result.LanguageCode.Value);

        Assert.Equal(
            15000m,
            result.SalaryRange.Min);

        Assert.Equal(
            22000m,
            result.SalaryRange.Max);

        Assert.Equal(
            CurrencyCode.PLN,
            result.SalaryRange.Currency);
    }

    [Fact]
    public void Create_should_throw_validation_exception_when_all_data_is_invalid()
    {
        var command = new UpdateJobPost(
            JobPostId: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            Title: "",
            Summary: new string('x', 5001),
            Description: new string('x', 5001),
            Responsibilities:
            [
                ""
            ],
            Requirements:
            [
                ""
            ],
            Skills:
            [
                ""
            ],
            Location: new string('x', 301),
            CountryCode: "",
            LanguageCode: "",
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Hybrid,
            CurrencyCode: CurrencyCode.PLN,
            SalaryMin: 22000m,
            SalaryMax: 15000m,
            ModifiedBy: Guid.NewGuid());

        var exception = Assert.Throws<ValidationException>(
            () => JobPostDataFactory.Create(command));

        Assert.NotEmpty(exception.Errors);

        Assert.Contains(
            PostTitle.RequiredMessage,
            exception.Errors);

        Assert.Contains(
            LongText.MaxLengthMessage,
            exception.Errors);

        Assert.Contains(
            JobLocation.MaxLengthMessage,
            exception.Errors);

        Assert.Contains(
            EntryText.RequiredMessage,
            exception.Errors);

        Assert.Contains(
            SalaryRange.MinimumExceedsMaximumMessage,
            exception.Errors);

        Assert.Contains(
            CountryCode.RequiredMessage,
            exception.Errors);

        Assert.Contains(
            LanguageCode.RequiredMessage,
            exception.Errors);
    }
}