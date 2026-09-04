using HrAgencySystem.Api.Endpoints.JobPosting.Maps;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.IntegrationTests.JobPosting;

internal static class JobPostingTestData
{
    public static CreatePostRequest CreateRequest(
        Guid? jobDescriptionId = null,
        Guid? recruiterId = null) =>
        new(
            JobDescriptionId: jobDescriptionId ?? Guid.NewGuid(),
            Title: "Senior .NET Developer",
            Summary: "Senior .NET Developer position",
            Description: "We are looking for an experienced .NET Developer.",
            Responsibilities:
            [
                "Design and develop applications",
                "Review code",
                "Collaborate with the team"
            ],
            Requirements:
            [
                "5+ years of experience",
                "Good knowledge of C#",
                "Good knowledge of PostgreSQL"
            ],
            Skills:
            [
                "C#",
                ".NET",
                "PostgreSQL",
                "Docker"
            ],
            Location: "Opole",
            CountryCode: "PL",
            LanguageCode: "PL",
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Hybrid,
            CurrencyCode: CurrencyCode.PLN,
            SalaryMin: 15_000,
            SalaryMax: 22_000,
            RecruiterId: recruiterId ?? Guid.NewGuid());
    

    public static UpdateJobPostRequest UpdateRequest() =>
        new(
            Title: "Senior Backend .NET Developer",
            Summary: "Updated senior backend developer position",
            Description: "We are looking for an experienced backend .NET developer.",
            Responsibilities:
            [
                "Develop backend applications",
                "Review code",
                "Mentor developers"
            ],
            Requirements:
            [
                "6+ years of experience",
                "Advanced C# knowledge",
                "PostgreSQL experience"
            ],
            Skills:
            [
                "C#",
                ".NET 10",
                "PostgreSQL",
                "Docker",
                "Kubernetes"
            ],
            Location: "Wrocław",
            CountryCode: "PL",
            LanguageCode: "PL",
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Remote,
            CurrencyCode: CurrencyCode.PLN,
            SalaryMin: 18_000,
            SalaryMax: 25_000);


    public static string InvalidTitle =>
        string.Empty;
}