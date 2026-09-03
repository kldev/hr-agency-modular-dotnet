using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class JobPostScenario(IMessageBus bus)
{
    public async Task<IReadOnlyList<Guid>> Create(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId)
    {
        var createdBy = recruiterId;

        var posts = new[]
        {
            CreatePolish(jobDescriptionId, organizationId, recruiterId, createdBy),
            CreateEnglish(jobDescriptionId, organizationId, recruiterId, createdBy),
            CreateFrench(jobDescriptionId, organizationId, recruiterId, createdBy),
            CreateBelgian(jobDescriptionId, organizationId, recruiterId, createdBy),
            CreateGerman(jobDescriptionId, organizationId, recruiterId, createdBy),
            CreateCzech(jobDescriptionId, organizationId, recruiterId, createdBy)
        };

        var ids = new List<Guid>(posts.Length);

        foreach (var post in posts)
        {
            var result = await bus.InvokeAsync<Guid>(post);
            ids.Add(result);
        }

        return ids;
    }

    private static CreateJobPost CreatePolish(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy)
        => Create(
            jobDescriptionId,
            organizationId,
            recruiterId,
            createdBy,
            "Java Backend Developer",
            "Szukamy doświadczonego programisty Java Backend.",
            "Dołącz do naszego zespołu inżynierskiego jako doświadczony Java Backend Developer.",
            "PL",
            "PLN",
            "PL",
            salaryMin: 12000,
            salaryMax: 18000,
            responsibilities:
            [
                "Projektowanie i rozwój aplikacji backendowych",
                "Tworzenie i utrzymywanie REST API",
                "Pisanie testów jednostkowych i integracyjnych",
                "Udział w przeglądach kodu",
                "Współpraca z programistami frontendowymi",
                "Monitorowanie i poprawa wydajności aplikacji"
            ],
            requirements:
            [
                "Minimum 3 lata doświadczenia w pracy z Javą",
                "Praktyczne doświadczenie ze Spring Boot",
                "Dobra znajomość relacyjnych baz danych",
                "Doświadczenie w tworzeniu REST API",
                "Umiejętność pracy w zespole",
                "Dobra znajomość języka angielskiego"
            ]);

    private static CreateJobPost CreateEnglish(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy)
        => Create(
            jobDescriptionId,
            organizationId,
            recruiterId,
            createdBy,
            "Java Backend Developer",
            "We are looking for an experienced Java Backend Developer.",
            "Join our engineering team as an experienced Java Backend Developer.",
            "EN",
            "EUR",
            "EN",
            salaryMin: 2999,
            salaryMax: 4999,
            responsibilities:
            [
                "Design and develop backend applications",
                "Develop and maintain REST APIs",
                "Write unit and integration tests",
                "Participate in code reviews",
                "Collaborate with frontend developers",
                "Monitor and improve application performance"
            ],
            requirements:
            [
                "At least 3 years of professional experience with Java",
                "Practical experience with Spring Boot",
                "Good knowledge of relational databases",
                "Experience developing REST APIs",
                "Ability to work effectively in a team",
                "Good command of English"
            ]);

    private static CreateJobPost CreateFrench(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy)
        => Create(
            jobDescriptionId,
            organizationId,
            recruiterId,
            createdBy,
            "Développeur Backend Java",
            "Nous recherchons un développeur Backend Java expérimenté.",
            "Rejoignez notre équipe d'ingénierie en tant que développeur Backend Java expérimenté.",
            "FR",
            "EUR",
            "FR",
            salaryMin: 3200,
            salaryMax: 4000,
            responsibilities:
            [
                "Concevoir et développer des applications backend",
                "Développer et maintenir des API REST",
                "Écrire des tests unitaires et d'intégration",
                "Participer aux revues de code",
                "Collaborer avec les développeurs frontend",
                "Surveiller et améliorer les performances des applications"
            ],
            requirements:
            [
                "Au moins 3 ans d'expérience professionnelle en Java",
                "Expérience pratique avec Spring Boot",
                "Bonne connaissance des bases de données relationnelles",
                "Expérience dans le développement d'API REST",
                "Capacité à travailler efficacement en équipe",
                "Bonne maîtrise de l'anglais"
            ]);

    private static CreateJobPost CreateBelgian(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy)
        => Create(
            jobDescriptionId,
            organizationId,
            recruiterId,
            createdBy,
            "Java Backend Developer",
            "Wij zijn op zoek naar een ervaren Java Backend Developer.",
            "Sluit je aan bij ons engineeringteam als ervaren Java Backend Developer.",
            "BE",
            "EUR",
            "NL",
            salaryMin: 3200,
            salaryMax: 4000,
            responsibilities:
            [
                "Backendapplicaties ontwerpen en ontwikkelen",
                "REST API's ontwikkelen en onderhouden",
                "Unit- en integratietests schrijven",
                "Deelnemen aan code reviews",
                "Samenwerken met frontendontwikkelaars",
                "De prestaties van applicaties monitoren en verbeteren"
            ],
            requirements:
            [
                "Minimaal 3 jaar professionele ervaring met Java",
                "Praktische ervaring met Spring Boot",
                "Goede kennis van relationele databases",
                "Ervaring met het ontwikkelen van REST API's",
                "Vermogen om effectief in teamverband te werken",
                "Goede beheersing van het Engels"
            ]);

    private static CreateJobPost CreateGerman(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy)
        => Create(
            jobDescriptionId,
            organizationId,
            recruiterId,
            createdBy,
            "Java Backend Entwickler",
            "Wir suchen einen erfahrenen Java Backend Entwickler.",
            "Verstärken Sie unser Engineering-Team als erfahrener Java Backend Entwickler.",
            "DE",
            "EUR",
            "DE",
            salaryMin: 3500,
            salaryMax: 5000,
            responsibilities:
            [
                "Backend-Anwendungen entwerfen und entwickeln",
                "REST-APIs entwickeln und warten",
                "Unit- und Integrationstests schreiben",
                "An Code-Reviews teilnehmen",
                "Mit Frontend-Entwicklern zusammenarbeiten",
                "Die Anwendungsleistung überwachen und verbessern"
            ],
            requirements:
            [
                "Mindestens 3 Jahre Berufserfahrung mit Java",
                "Praktische Erfahrung mit Spring Boot",
                "Gute Kenntnisse relationaler Datenbanken",
                "Erfahrung in der Entwicklung von REST-APIs",
                "Fähigkeit zur effektiven Zusammenarbeit im Team",
                "Gute Englischkenntnisse"
            ]);

    private static CreateJobPost CreateCzech(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy)
        => Create(
            jobDescriptionId,
            organizationId,
            recruiterId,
            createdBy,
            "Java Backend Developer",
            "Hledáme zkušeného Java Backend Developera.",
            "Připojte se k našemu vývojovému týmu jako zkušený Java Backend Developer.",
            "CZ",
            "EUR",
            "CS",
            salaryMin: 3000,
            salaryMax: 3500,
            responsibilities:
            [
                "Navrhovat a vyvíjet backendové aplikace",
                "Vyvíjet a udržovat REST API",
                "Psát unit a integrační testy",
                "Účastnit se code review",
                "Spolupracovat s frontendovými vývojáři",
                "Monitorovat a zlepšovat výkon aplikací"
            ],
            requirements:
            [
                "Minimálně 3 roky profesionálních zkušeností s Javou",
                "Praktické zkušenosti se Spring Boot",
                "Dobrá znalost relačních databází",
                "Zkušenosti s vývojem REST API",
                "Schopnost efektivně pracovat v týmu",
                "Dobrá znalost angličtiny"
            ]);

    private static CreateJobPost Create(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid recruiterId,
        Guid createdBy,
        string title,
        string summary,
        string description,
        string countryCode,
        string currencyCode,
        string languageCode,
        decimal salaryMin,
        decimal salaryMax,
        IReadOnlyList<string> responsibilities,
        IReadOnlyList<string> requirements)
        => new(
            JobDescriptionId: jobDescriptionId,
            OrganizationId: organizationId,
            Title: title,
            Summary: summary,
            Description: description,
            Responsibilities: responsibilities,
            Requirements: requirements,
            Skills:
            [
                "Java 21",
                "Spring Boot",
                "PostgreSQL",
                "REST API",
                "Docker",
                "Git"
            ],
            Location: "Opole",
            CountryCode: countryCode,
            LanguageCode: languageCode,
            EmploymentType: EmploymentType.FullTime,
            WorkMode: WorkMode.Hybrid,
            CurrencyCode: Enum.Parse<CurrencyCode>(currencyCode),
            SalaryMin: salaryMin,
            SalaryMax: salaryMax,
            RecruiterId: recruiterId,
            CreatedBy: createdBy);
}