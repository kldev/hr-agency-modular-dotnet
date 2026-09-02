using HrAgencySystem.Api.Endpoints.JobDescription.Maps;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.IntegrationTests.JobDescription;

internal static class JobDescriptionTestData
{
    public static CreateJobDescriptionRequest CreateRequest(
        Guid? companyId = null,
        Guid? recruiterId = null) =>
        new(
            CompanyId: companyId ?? Guid.NewGuid(),
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
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Hybrid,
            CurrencyCode: CurrencyCode.PLN,
            SalaryMin: 15_000,
            SalaryMax: 22_000,
            RecruiterId: recruiterId ?? Guid.NewGuid());

    public static UpdateJobDescriptionRequest UpdateRequest() =>
        new(
            Title: "Senior Backend .NET Developer",
            Summary: "Updated job description",
            Description: "Updated description for the position.",
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
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Remote,
            CurrencyCode: CurrencyCode.PLN,
            SalaryMin: 18_000,
            SalaryMax: 25_000);

    public static AssignRecruiter CreateAssignRecruiterRequest(
        Guid? recruiterId = null) =>
        new(recruiterId ?? Guid.NewGuid());

    public static string InvalidTitle =>
        string.Empty;
}