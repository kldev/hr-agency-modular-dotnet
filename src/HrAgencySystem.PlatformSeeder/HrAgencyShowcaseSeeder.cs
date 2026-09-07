using System.Diagnostics;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.PlatformSeeder.Scenario;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HrAgencyShowcaseSeeder(
    IMessageBus bus,
    IQuerySession session,
    ILogger<HrAgencyShowcaseSeeder> logger) : IPlatformSeeder
{
    private const int ProjectionDelayMs = 5_000;

    public async Task Seed()
    {
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Starting HR Agency showcase seeding");

        var owner = await new OwnerScenario(bus).Create();

        logger.LogInformation(
            "Platform owner created: {PlatformOwnerId}",
            owner.PlatformOwnerId);

        await SeedAgency(owner.PlatformOwnerId, new SeedConfig());

        await SeedAgency(
            owner.PlatformOwnerId,
            new SeedConfig(
                Name: "Flex Jobs",
                Slug: "flex-jobs",
                UsersCount: 50,
                CompaniesCount: 999));

        logger.LogInformation(
            "HR Agency showcase seeding completed in {Elapsed}",
            stopwatch.Elapsed);
    }

    public async Task SeedApplicants(int count)
    {
        logger.LogInformation(
            "Starting applicant seeding: {Count} applicants per organization",
            count);

        var organizations = await session
            .Query<OrganizationCreated>()
            .ToListAsync();

        logger.LogInformation(
            "Found {OrganizationCount} organizations",
            organizations.Count);

        foreach (var organization in organizations)
        {
            logger.LogInformation(
                "Generating {Count} applicants for organization {OrganizationId}",
                count,
                organization.OrganizationId);

            await GenerateApplicants(count);
        }

        logger.LogInformation("Applicant seeding completed");
    }

    public async Task SeedShowcase()
    {
        logger.LogInformation("Starting showcase seeding");

        var organizations = await session
            .Query<OrganizationCreated>()
            .ToListAsync();

        logger.LogInformation(
            "Found {OrganizationCount} organizations for showcase",
            organizations.Count);

        foreach (var organization in organizations)
        {
            logger.LogInformation(
                "Creating showcase data for organization {OrganizationId}",
                organization.OrganizationId);

            var userIds = await session
                .Query<UserCreated>()
                .Where(z => z.OrganizationId == organization.OrganizationId)
                .Select(z => z.UserId)
                .ToListAsync();

            var companyIds = await session
                .Query<CompanyCreated>()
                .Where(z => z.OrganizationId == organization.OrganizationId)
                .Select(z => z.CompanyId)
                .ToListAsync();

            logger.LogInformation(
                "Organization {OrganizationId}: found {UserCount} users and {CompanyCount} companies",
                organization.OrganizationId,
                userIds.Count,
                companyIds.Count);

            await new ModernWebDeveloperScenario(bus)
                .Create(
                    organization.OrganizationId,
                    userIds,
                    companyIds);
        }

        logger.LogInformation("Creating showcase job applications");

        await new ApplyToJobPostScenario(bus, session)
            .ExecuteShowcase();

        logger.LogInformation("Showcase seeding completed");
    }

    private async Task SeedAgency(Guid ownerId, SeedConfig config)
    {
        logger.LogInformation(
            "Starting agency seed: {Name} ({Slug}), Users: {UsersCount}, Companies: {CompaniesCount}",
            config.Name,
            config.Slug,
            config.UsersCount,
            config.CompaniesCount);

        var organization = await new OrganizationScenario(bus)
            .Create(ownerId, config.Name, config.Slug);

        logger.LogInformation(
            "Organization created: {OrganizationId} ({Slug})",
            organization.OrganizationId,
            organization.Slug);

        var userIds = await CreateUsers(config, organization);

        logger.LogInformation(
            "Created {UserCount} users for organization {OrganizationId}",
            userIds.Count,
            organization.OrganizationId);

        await WaitForProjections();

        var companyIds = await CreateCompanies(config, organization, userIds);

        logger.LogInformation(
            "Created {CompanyCount} companies for organization {OrganizationId}",
            companyIds.Count,
            organization.OrganizationId);

        await WaitForProjections();

        logger.LogInformation(
            "Creating job descriptions for organization {OrganizationId}",
            organization.OrganizationId);

        await CreateProductionJobDescription(
            organization,
            userIds,
            companyIds);

        var javaJobDescriptionId =
            await CreateTechnicalJobsDescription(
                organization,
                userIds,
                companyIds);

        await CreateMoreJobPosting(
            javaJobDescriptionId,
            organization,
            userIds);

        await CreateModernDeveloperPosts(
            organization,
            userIds,
            companyIds);

        logger.LogInformation(
            "Job descriptions and job posts created for organization {OrganizationId}",
            organization.OrganizationId);

        await WaitForProjections();
        await WaitForProjections();
        
        await PostToChannel(userIds);
        
        await WaitForProjections();
        await WaitForProjections();
        logger.LogInformation(
            "Job posts published to channels for organization {OrganizationId}",
            organization.OrganizationId);

        await GenerateApplicants(20);

        logger.LogInformation(
            "Generated applicants for organization {OrganizationId}",
            organization.OrganizationId);

        logger.LogInformation(
            "Waiting for applicant projections before creating interviews for organization {OrganizationId}",
            organization.OrganizationId);
        
        await WaitForProjections();
        
        logger.LogInformation(
            "Creating interviews for organization {OrganizationId}",
            organization.OrganizationId);

      

        await new InterviewsScenario(bus, session)
            .SeedAsync(
                organization.OrganizationId,
                200);

        logger.LogInformation(
            "Agency seed completed: {Slug} ({OrganizationId})",
            organization.Slug,
            organization.OrganizationId);
    }

    #region Tasks

    private async Task PostToChannel(IReadOnlyList<Guid> userIds)
    {
        logger.LogDebug(
            "Posting jobs to random channels for {UserCount} users",
            userIds.Count);

        await new PostJobToRandomChannelScenario(bus, session)
            .Execute(userIds);
    }

    private async Task GenerateApplicants(int count = 500)
    {
        logger.LogDebug(
            "Generating {ApplicantCount} applicants",
            count);

        await new ApplyToJobPostScenario(bus, session)
            .Execute(count);
    }

    private async Task CreateModernDeveloperPosts(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        logger.LogDebug(
            "Creating modern developer job posts for organization {OrganizationId}",
            organization.OrganizationId);

        await new ModernWebDeveloperScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                companyIds);
    }

    private async Task CreateMoreJobPosting(
        Guid javaJobDescriptionId,
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds)
    {
        logger.LogDebug(
            "Creating additional job post for job description {JobDescriptionId}",
            javaJobDescriptionId);

        await new JobPostScenario(bus)
            .Create(
                javaJobDescriptionId,
                organization.OrganizationId,
                userIds.First());
    }

    private async Task<Guid> CreateTechnicalJobsDescription(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        logger.LogDebug(
            "Creating technical job description for organization {OrganizationId}",
            organization.OrganizationId);

        return await new TechnicalJobDescriptionScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                companyIds);
    }

    private async Task CreateProductionJobDescription(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        logger.LogDebug(
            "Creating production job description for organization {OrganizationId}",
            organization.OrganizationId);

        await new ProductionJobDescriptionScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                companyIds);
    }

    private async Task<IReadOnlyList<Guid>> CreateCompanies(
        SeedConfig config,
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds)
    {
        return await new CompanyScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                config.CompaniesCount);
    }

    private async Task<IReadOnlyList<Guid>> CreateUsers(
        SeedConfig config,
        OrganizationScenario.OrganizationData organization)
    {
        return await new UserScenario(bus)
            .Create(
                organization,
                config.UsersCount);
    }

    #endregion

    private async Task WaitForProjections()
    {
        logger.LogDebug(
            "Waiting {ProjectionDelayMs}ms for projections",
            ProjectionDelayMs);

        await Task.Delay(ProjectionDelayMs);
    }

    private sealed record SeedConfig(
        string Name = "Hr Agency",
        string Slug = "hr-agency",
        int UsersCount = 20,
        int CompaniesCount = 101);
}