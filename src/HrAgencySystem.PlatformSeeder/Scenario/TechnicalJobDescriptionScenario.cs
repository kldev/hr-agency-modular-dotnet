using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class TechnicalJobDescriptionScenario(IMessageBus bus)
{
    public async Task Create(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        if (companyIds.Count == 0 || userIds.Count == 0)
            return;

        var companyIndex = 0;
        var userIndex = 0;

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Java Backend Developer",
                null,
                "We are looking for an experienced Java Backend Developer to join our engineering team.",
                [
                    "Design and develop backend applications",
                    "Develop and maintain REST APIs",
                    "Write unit and integration tests",
                    "Participate in code reviews",
                    "Cooperate with frontend developers",
                    "Monitor and improve application performance"
                ],
                [
                    "At least 3 years of experience with Java",
                    "Practical experience with Spring Boot",
                    "Good knowledge of relational databases",
                    "Experience with REST APIs",
                    "Ability to work in a team",
                    "Good English"
                ],
                [
                    "Java 21",
                    "Spring Boot",
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
                12000m,
                22000m,
                userIds[0],
                userIds[0]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Senior Full Stack Developer",
                null,
                "Development of modern business applications using Java, Spring Boot and React.",
                [
                    "Develop backend and frontend applications",
                    "Design REST APIs",
                    "Implement new business features",
                    "Review pull requests",
                    "Participate in technical discussions",
                    "Maintain application quality"
                ],
                [
                    "At least 4 years of commercial software development",
                    "Strong Java knowledge",
                    "Experience with React and TypeScript",
                    "Good understanding of REST architecture",
                    "Knowledge of PostgreSQL",
                    "Ability to work independently"
                ],
                [
                    "Java",
                    "Spring Boot",
                    "React",
                    "TypeScript",
                    "PostgreSQL",
                    "Docker"
                ],
                "Wrocław",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                14000m,
                24000m,
                userIds[1 % userIds.Count],userIds[1 % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Frontend Developer",
                null,
                "Join a frontend team building modern web applications for international customers.",
                [
                    "Develop modern web applications",
                    "Create reusable React components",
                    "Integrate applications with REST APIs",
                    "Write automated tests",
                    "Optimize application performance",
                    "Cooperate with UX and backend teams"
                ],
                [
                    "At least 2 years of experience with React",
                    "Good knowledge of TypeScript",
                    "Good understanding of HTML and CSS",
                    "Experience with REST APIs",
                    "Knowledge of Git",
                    "English at communicative level"
                ],
                [
                    "TypeScript",
                    "React",
                    "HTML",
                    "CSS",
                    "REST API",
                    "Git"
                ],
                "Katowice",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Remote,
                CurrencyCode.PLN,
                10000m,
                18000m,
                userIds[2 % userIds.Count],
                userIds[2 % userIds.Count]
            )
        );

        await Create(
            
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "DevOps Engineer",
                null,
                "We are looking for a DevOps Engineer responsible for our cloud infrastructure and deployment platforms.",
                [
                    "Maintain cloud infrastructure",
                    "Develop CI/CD pipelines",
                    "Automate deployment processes",
                    "Monitor production environments",
                    "Improve system reliability",
                    "Cooperate with development teams"
                ],
                [
                    "Experience with Kubernetes",
                    "Strong Docker knowledge",
                    "Experience with Azure or another cloud platform",
                    "Knowledge of Terraform",
                    "Experience with CI/CD",
                    "Good Linux knowledge"
                ],
                [
                    "Kubernetes",
                    "Docker",
                    "Azure",
                    "Terraform",
                    "CI/CD",
                    "Linux"
                ],
                "Warsaw",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                15000m,
                25000m,
                userIds[3 % userIds.Count],
                userIds[3 % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "QA Automation Engineer",
                null,
                "Development and maintenance of automated tests for web and backend applications.",
                [
                    "Develop automated tests",
                    "Maintain existing test suites",
                    "Test REST APIs",
                    "Analyze test results",
                    "Report and verify defects",
                    "Cooperate with developers"
                ],
                [
                    "At least 2 years of experience in test automation",
                    "Knowledge of Java",
                    "Experience with Selenium or Playwright",
                    "Experience with REST API testing",
                    "Knowledge of JUnit",
                    "Analytical thinking"
                ],
                [
                    "Java",
                    "Selenium",
                    "Playwright",
                    "REST Assured",
                    "JUnit",
                    "CI/CD"
                ],
                "Poznań",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                10000m,
                17000m,
                userIds[4 % userIds.Count],
                userIds[4 % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Business Analyst",
                null,
                "Work closely with business stakeholders and development teams to translate business needs into actionable requirements.",
                [
                    "Analyze business requirements",
                    "Prepare functional specifications",
                    "Model business processes",
                    "Cooperate with stakeholders",
                    "Support development teams",
                    "Participate in acceptance testing"
                ],
                [
                    "Experience in business analysis",
                    "Knowledge of BPMN or UML",
                    "Good SQL knowledge",
                    "Experience with Jira and Confluence",
                    "Strong communication skills",
                    "Ability to translate business needs into technical requirements"
                ],
                [
                    "Requirements analysis",
                    "UML",
                    "BPMN",
                    "SQL",
                    "Jira",
                    "Confluence"
                ],
                "Wrocław",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                10000m,
                18000m,
                userIds[5 % userIds.Count],
                userIds[5 % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Data Engineer",
                null,
                "Build and maintain data pipelines supporting analytics and business intelligence.",
                [
                    "Build and maintain data pipelines",
                    "Develop ETL processes",
                    "Optimize SQL queries",
                    "Integrate data from multiple sources",
                    "Monitor data quality",
                    "Cooperate with data analysts"
                ],
                [
                    "Experience with Python",
                    "Strong SQL knowledge",
                    "Experience with PostgreSQL",
                    "Knowledge of ETL processes",
                    "Experience with Airflow or similar tools",
                    "Analytical thinking"
                ],
                [
                    "Python",
                    "SQL",
                    "PostgreSQL",
                    "Apache Kafka",
                    "ETL",
                    "Airflow"
                ],
                "Kraków",
                "PL",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                CurrencyCode.PLN,
                12000m,
                20000m,
                userIds[6 % userIds.Count],
                userIds[6 % userIds.Count]
            )
        );
    }

    private async Task Create(
        Guid userId,
        CreateJobDescription request)
    {
        await bus.InvokeAsync(
            request with
            { 
                RecruiterId = userId
            });
    }
}
