using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class ModernWebDeveloperScenario(IMessageBus bus)
{
    private static readonly Random Random = new();

    
    public async Task<IReadOnlyList<Guid>> Create(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        if (companyIds.Count == 0 || userIds.Count == 0)
            return [];

        var companyIndex = 0;
        var userIndex = 0;

        var jobDescriptions = new[]
        {
            await CreateCSharpDeveloper(
                organizationId,
                companyIds[Random.Next(0, companyIds.Count)],
                userIds[Random.Next(0, userIds.Count)]),

            await CreateNextJsDeveloper(
                organizationId,
                companyIds[Random.Next(0, companyIds.Count)],
                userIds[Random.Next(0, userIds.Count)]),

            await CreateNodeJsDeveloper(
                organizationId,
                companyIds[Random.Next(0, companyIds.Count)],
                userIds[Random.Next(0, userIds.Count)]),
        };

        await Task.Delay(TimeSpan.FromSeconds(5));

        var jobPostIds = new List<Guid>();

        foreach (var jobDescription in jobDescriptions)
        {
            var recruiterId = jobDescription.RecruiterId;

            jobPostIds.Add(
                await CreateJobPost(
                    jobDescription.JobDescriptionId,
                    organizationId,
                    recruiterId,
                    CreatePolishPost(jobDescription)));

            jobPostIds.Add(
                await CreateJobPost(
                    jobDescription.JobDescriptionId,
                    organizationId,
                    recruiterId,
                    CreateEnglishPost(jobDescription)));
        }

        return jobPostIds;
    }

    private async Task<JobDescriptionSeedResult> CreateCSharpDeveloper(
        Guid organizationId,
        Guid companyId,
        Guid recruiterId)
    {
        var result = await bus.InvokeAsync<JobDescriptionCreated>(
            new CreateJobDescription(
                organizationId,
                companyId,
                "C# Developer",
                $"Company {companyId}",
                "We are looking for an experienced C# Developer to join our engineering team and build modern backend applications using .NET.",
                [
                    "Design and develop backend applications using C# and .NET",
                    "Develop and maintain REST APIs",
                    "Implement business logic and domain features",
                    "Write unit and integration tests",
                    "Participate in code reviews",
                    "Cooperate with frontend developers and other engineering teams"
                ],
                [
                    "At least 3 years of commercial experience with C#",
                    "Practical experience with .NET",
                    "Good knowledge of ASP.NET Core",
                    "Experience with REST APIs",
                    "Good knowledge of relational databases",
                    "Experience with automated testing",
                    "Ability to work effectively in a team",
                    "Good command of English"
                ],
                [
                    "C#",
                    ".NET",
                    "ASP.NET Core",
                    "PostgreSQL",
                    "REST API",
                    "Docker",
                    "Git"
                ],
                "Opole",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                14000m,
                22000m,
                recruiterId,
                recruiterId)
            with
            {
                RecruiterId = recruiterId
            });

        return new JobDescriptionSeedResult(
            result.JobDescriptionId,
            recruiterId, result.Title );
    }

    private async Task<JobDescriptionSeedResult> CreateNextJsDeveloper(
        Guid organizationId,
        Guid companyId,
        Guid recruiterId)
    {
        var result = await bus.InvokeAsync<JobDescriptionCreated>(
            new CreateJobDescription(
                organizationId,
                companyId,
                "NextJS Developer",
                null,
                "We are looking for a NextJS Developer to build fast, scalable and user-friendly web applications for our customers.",
                [
                    "Develop modern web applications using NextJS and React",
                    "Create reusable and maintainable frontend components",
                    "Integrate frontend applications with REST APIs",
                    "Optimize application performance and user experience",
                    "Write unit and integration tests",
                    "Cooperate with backend developers and UX designers"
                ],
                [
                    "At least 2 years of commercial experience with React",
                    "Good knowledge of NextJS",
                    "Strong TypeScript skills",
                    "Good understanding of HTML and CSS",
                    "Experience integrating REST APIs",
                    "Knowledge of frontend testing",
                    "Good understanding of Git",
                    "Good command of English"
                ],
                [
                    "NextJS",
                    "React",
                    "TypeScript",
                    "JavaScript",
                    "HTML",
                    "CSS",
                    "REST API",
                    "Git"
                ],
                "Opole",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Remote,
                CurrencyCode.PLN,
                12000m,
                20000m,
                recruiterId,
                recruiterId)
            with
            {
                RecruiterId = recruiterId
            });

        return new JobDescriptionSeedResult(
            result.JobDescriptionId,
            recruiterId, result.Title);
    }

    private async Task<JobDescriptionSeedResult> CreateNodeJsDeveloper(
        Guid organizationId,
        Guid companyId,
        Guid recruiterId)
    {
        var result = await bus.InvokeAsync<JobDescriptionCreated>(
            new CreateJobDescription(
                organizationId,
                companyId,
                "NodeJS Developer",
                null,
                "We are looking for an experienced NodeJS Developer to build scalable backend services and APIs for modern web applications.",
                [
                    "Design and develop backend services using NodeJS",
                    "Develop and maintain REST APIs",
                    "Implement scalable application architecture",
                    "Integrate applications with databases and external services",
                    "Write unit and integration tests",
                    "Monitor and improve application performance"
                ],
                [
                    "At least 3 years of professional experience with NodeJS",
                    "Strong JavaScript or TypeScript knowledge",
                    "Experience with REST API development",
                    "Good knowledge of relational databases",
                    "Experience with backend application architecture",
                    "Knowledge of automated testing",
                    "Experience with Docker",
                    "Good command of English"
                ],
                [
                    "NodeJS",
                    "TypeScript",
                    "JavaScript",
                    "REST API",
                    "PostgreSQL",
                    "Docker",
                    "Git"
                ],
                "Opole",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                13000m,
                21000m,
                recruiterId,
                recruiterId)
            with
            {
                RecruiterId = recruiterId
            });

        return new JobDescriptionSeedResult(
            result.JobDescriptionId,
            recruiterId, result.Title);
    }

    private static CreateJobPost CreatePolishPost(
        JobDescriptionSeedResult jobDescription)
    {
        return jobDescription switch
        {
            { Title: "C# Developer" } => CreatePolish(
                jobDescription.JobDescriptionId,
                "C# Developer",
                "Szukamy doświadczonego programisty C# do naszego zespołu inżynierskiego.",
                "Dołącz do naszego zespołu i rozwijaj nowoczesne aplikacje backendowe wykorzystujące C# i .NET.",
                14000,
                22000,
                [
                    "Projektowanie i rozwój aplikacji backendowych w C# i .NET",
                    "Tworzenie i utrzymywanie REST API",
                    "Implementacja logiki biznesowej",
                    "Pisanie testów jednostkowych i integracyjnych",
                    "Udział w code review",
                    "Współpraca z programistami frontendowymi"
                ],
                [
                    "Minimum 3 lata komercyjnego doświadczenia z C#",
                    "Praktyczna znajomość .NET",
                    "Dobra znajomość ASP.NET Core",
                    "Doświadczenie w tworzeniu REST API",
                    "Dobra znajomość relacyjnych baz danych",
                    "Doświadczenie w testach automatycznych"
                ]),

            { Title: "NextJS Developer" } => CreatePolish(
                jobDescription.JobDescriptionId,
                "NextJS Developer",
                "Szukamy programisty NextJS do tworzenia nowoczesnych aplikacji webowych.",
                "Dołącz do naszego zespołu frontendowego i twórz szybkie oraz skalowalne aplikacje webowe.",
                12000,
                20000,
                [
                    "Tworzenie aplikacji webowych z wykorzystaniem NextJS i React",
                    "Tworzenie komponentów wielokrotnego użytku",
                    "Integracja aplikacji z REST API",
                    "Optymalizacja wydajności aplikacji",
                    "Pisanie testów automatycznych",
                    "Współpraca z backendem i UX"
                ],
                [
                    "Minimum 2 lata komercyjnego doświadczenia z React",
                    "Dobra znajomość NextJS",
                    "Bardzo dobra znajomość TypeScript",
                    "Znajomość HTML i CSS",
                    "Doświadczenie w integracji REST API",
                    "Znajomość testów frontendowych"
                ]),

            { Title: "NodeJS Developer" } => CreatePolish(
                jobDescription.JobDescriptionId,
                "NodeJS Developer",
                "Szukamy doświadczonego programisty NodeJS do tworzenia skalowalnych usług backendowych.",
                "Dołącz do naszego zespołu backendowego i rozwijaj nowoczesne usługi oraz API w NodeJS.",
                13000,
                21000,
                [
                    "Projektowanie i rozwój usług backendowych w NodeJS",
                    "Tworzenie i utrzymywanie REST API",
                    "Projektowanie skalowalnej architektury aplikacji",
                    "Integracja z bazami danych i usługami zewnętrznymi",
                    "Pisanie testów jednostkowych i integracyjnych",
                    "Monitorowanie i poprawa wydajności"
                ],
                [
                    "Minimum 3 lata profesjonalnego doświadczenia z NodeJS",
                    "Bardzo dobra znajomość JavaScript lub TypeScript",
                    "Doświadczenie w tworzeniu REST API",
                    "Dobra znajomość relacyjnych baz danych",
                    "Znajomość architektury aplikacji backendowych",
                    "Doświadczenie w testach automatycznych"
                ]),

            _ => throw new InvalidOperationException(
                $"Unsupported Job Description: {jobDescription.Title}")
        };
    }

    private static CreateJobPost CreateEnglishPost(
        JobDescriptionSeedResult jobDescription)
    {
        return jobDescription switch
        {
            { Title: "C# Developer" } => CreateEnglish(
                jobDescription.JobDescriptionId,
                "C# Developer",
                "We are looking for an experienced C# Developer to join our engineering team.",
                "Join our engineering team and build modern backend applications using C# and .NET.",
                3000,
                5000,
                [
                    "Design and develop backend applications using C# and .NET",
                    "Develop and maintain REST APIs",
                    "Implement business logic",
                    "Write unit and integration tests",
                    "Participate in code reviews",
                    "Collaborate with frontend developers"
                ],
                [
                    "At least 3 years of commercial experience with C#",
                    "Practical experience with .NET",
                    "Good knowledge of ASP.NET Core",
                    "Experience with REST APIs",
                    "Good knowledge of relational databases",
                    "Experience with automated testing"
                ]),

            { Title: "NextJS Developer" } => CreateEnglish(
                jobDescription.JobDescriptionId,
                "NextJS Developer",
                "We are looking for a NextJS Developer to build modern web applications.",
                "Join our frontend team and build fast, scalable and user-friendly web applications.",
                2800,
                4500,
                [
                    "Develop web applications using NextJS and React",
                    "Create reusable frontend components",
                    "Integrate applications with REST APIs",
                    "Optimize application performance",
                    "Write automated tests",
                    "Collaborate with backend developers and UX designers"
                ],
                [
                    "At least 2 years of commercial experience with React",
                    "Good knowledge of NextJS",
                    "Strong TypeScript skills",
                    "Good knowledge of HTML and CSS",
                    "Experience integrating REST APIs",
                    "Knowledge of frontend testing"
                ]),

            { Title: "NodeJS Developer" } => CreateEnglish(
                jobDescription.JobDescriptionId,
                "NodeJS Developer",
                "We are looking for an experienced NodeJS Developer to build scalable backend services.",
                "Join our backend team and develop modern backend services and APIs using NodeJS.",
                2900,
                4800,
                [
                    "Design and develop backend services using NodeJS",
                    "Develop and maintain REST APIs",
                    "Design scalable application architecture",
                    "Integrate applications with databases and external services",
                    "Write unit and integration tests",
                    "Monitor and improve application performance"
                ],
                [
                    "At least 3 years of professional experience with NodeJS",
                    "Strong JavaScript or TypeScript knowledge",
                    "Experience with REST API development",
                    "Good knowledge of relational databases",
                    "Knowledge of backend application architecture",
                    "Experience with automated testing"
                ]),

            _ => throw new InvalidOperationException(
                $"Unsupported Job Description: {jobDescription.Title}")
        };
    }

    private async Task<Guid> CreateJobPost(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        CreateJobPost request)
    {
        return await bus.InvokeAsync<Guid>(
            request with
            {
                JobDescriptionId = jobDescriptionId,
                OrganizationId = organizationId,
                RecruiterId = recruiterId,
                CreatedBy = recruiterId
            });
    }

    private static CreateJobPost CreatePolish(
        Guid jobDescriptionId,
        string title,
        string summary,
        string description,
        decimal salaryMin,
        decimal salaryMax,
        IReadOnlyList<string> responsibilities,
        IReadOnlyList<string> requirements)
        => Create(
            jobDescriptionId,
            title,
            summary,
            description,
            "PL",
            "PLN",
            salaryMin,
            salaryMax,
            responsibilities,
            requirements);

    private static CreateJobPost CreateEnglish(
        Guid jobDescriptionId,
        string title,
        string summary,
        string description,
        decimal salaryMin,
        decimal salaryMax,
        IReadOnlyList<string> responsibilities,
        IReadOnlyList<string> requirements)
        => Create(
            jobDescriptionId,
            title,
            summary,
            description,
            "EN",
            "EUR",
            salaryMin,
            salaryMax,
            responsibilities,
            requirements);

    private static CreateJobPost Create(
        Guid jobDescriptionId,
        string title,
        string summary,
        string description,
        string countryCode,
        string currencyCode,
        decimal salaryMin,
        decimal salaryMax,
        IReadOnlyList<string> responsibilities,
        IReadOnlyList<string> requirements)
        => new(
            JobDescriptionId: jobDescriptionId,
            OrganizationId: Guid.Empty,
            Title: title,
            Summary: summary,
            Description: description,
            Responsibilities: responsibilities,
            Requirements: requirements,
            Skills: [],
            Location: "Opole",
            CountryCode: countryCode,
            LanguageCode: countryCode,
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Hybrid,
            CurrencyCode: Enum.Parse<CurrencyCode>(currencyCode),
            SalaryMin: salaryMin,
            SalaryMax: salaryMax,
            RecruiterId: Guid.Empty,
            CreatedBy: Guid.Empty);

    private sealed record JobDescriptionSeedResult(
        Guid JobDescriptionId,
        Guid RecruiterId,
        String Title)
    {
    }
}