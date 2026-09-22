using HrAgencySystem.PlatformSeeder.Scenario;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

public sealed partial class HrAgencyShowcaseSeeder
{
    private async Task SeedAgency(Guid ownerId, SeedConfig config)
    {
        logger.LogInformation(
            "Starting agency seed: {Name} ({Slug}), Users: {UsersCount}, Companies: {CompaniesCount}",
            config.Name,
            config.Slug,
            config.UsersCount,
            config.CompaniesCount
        );

        var organization = await new OrganizationScenario(bus).Create(
            ownerId,
            config.Name,
            config.Slug
        );

        logger.LogInformation(
            "Organization created: {OrganizationId} ({Slug})",
            organization.OrganizationId,
            organization.Slug
        );

        var userIds = await CreateUsers(config, organization);

        logger.LogInformation(
            "Created {UserCount} users for organization {OrganizationId}",
            userIds.Count,
            organization.OrganizationId
        );

        await WaitForProjections();

        var teamIds = await new TeamScenario(bus).Create(
            organization.OrganizationId,
            userIds,
            userIds[0]
        );

        logger.LogInformation(
            "Created {TeamCount} teams for organization {OrganizationId}",
            teamIds.Count,
            organization.OrganizationId
        );

        // The chart goes in right after the people, because every unit needs somebody to put in it
        // - and because the supervisor rule is what the next two plans are waiting on.
        var unitCount = await new OrgStructureScenario(bus).Create(
            organization.OrganizationId,
            organization.Slug,
            userIds,
            () => WaitForProjections()
        );

        logger.LogInformation(
            "Created {UnitCount} org units for organization {OrganizationId}",
            unitCount,
            organization.OrganizationId
        );

        // After the chart, because a month is approved by whoever is above the person in it.
        var filledSheets = await new TimeRecordScenario(bus, session).Seed(
            organization.OrganizationId,
            () => WaitForProjections()
        );

        logger.LogInformation(
            "Filled {SheetCount} time sheets for organization {OrganizationId}",
            filledSheets,
            organization.OrganizationId
        );

        var companyIds = await CreateCompanies(config, organization, userIds);

        logger.LogInformation(
            "Created {CompanyCount} companies for organization {OrganizationId}",
            companyIds.Count,
            organization.OrganizationId
        );

        await WaitForProjections();

        logger.LogInformation(
            "Creating job descriptions for organization {OrganizationId}",
            organization.OrganizationId
        );

        await CreateProductionJobDescription(organization, userIds, companyIds);

        var javaJobDescriptionId = await CreateTechnicalJobsDescription(
            organization,
            userIds,
            companyIds
        );

        await CreateMoreJobPosting(javaJobDescriptionId, organization, userIds);

        await CreateModernDeveloperPosts(organization, userIds, companyIds);

        logger.LogInformation(
            "Job descriptions and job posts created for organization {OrganizationId}",
            organization.OrganizationId
        );

        await WaitForProjections(times: 2);

        await PostToChannel(userIds);

        await WaitForProjections();

        logger.LogInformation(
            "Job posts published to channels for organization {OrganizationId}",
            organization.OrganizationId
        );

        await GenerateApplicants(20, true);

        logger.LogInformation(
            "Generated applicants for organization {OrganizationId}",
            organization.OrganizationId
        );

        await WaitForProjections();

        logger.LogInformation(
            "Creating interviews for organization {OrganizationId}",
            organization.OrganizationId
        );

        await new InterviewsScenario(bus, session, logger).SeedAsync(organization.OrganizationId, 200);

        logger.LogInformation(
            "Agency seed completed: {Slug} ({OrganizationId})",
            organization.Slug,
            organization.OrganizationId
        );

        await new TagsScenario(bus, session, logger).Seed(organization.OrganizationId, userIds);
    }

    private async Task SeedMinimalAgency(Guid ownerId, SeedConfig config)
    {
        var organization = await new OrganizationScenario(bus).Create(
            ownerId,
            config.Name,
            config.Slug
        );

        var userIds = await CreateUsers(config, organization);

        await new TeamScenario(bus).Create(organization.OrganizationId, userIds, userIds[0]);

        // The chart goes in right after the people, because every unit needs somebody to put in it
        // - and because the supervisor rule is what the next two plans are waiting on.
        var unitCount = await new OrgStructureScenario(bus).Create(
            organization.OrganizationId,
            organization.Slug,
            userIds,
            () => WaitForProjections()
        );

        logger.LogInformation(
            "Created {UnitCount} org units for organization {OrganizationId}",
            unitCount,
            organization.OrganizationId
        );

        await new TimeRecordScenario(bus, session).Seed(
            organization.OrganizationId,
            () => WaitForProjections()
        );

        var companyIds = await CreateCompanies(config, organization, userIds);

        await CreateModernDeveloperPosts(organization, userIds, companyIds);

        await PostToChannel(userIds);

        await GenerateApplicants(20, false);

        await new TagsScenario(bus, session, logger).Seed(organization.OrganizationId, userIds);
    }

    private async Task<IReadOnlyList<Guid>> CreateUsers(
        SeedConfig config,
        OrganizationScenario.OrganizationData organization
    )
    {
        return await new UserScenario(bus).Create(organization, config.UsersCount);
    }

    private async Task<IReadOnlyList<Guid>> CreateCompanies(
        SeedConfig config,
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds
    )
    {
        return await new CompanyScenario(bus, session).Create(
            organization.OrganizationId,
            userIds,
            config.CompaniesCount
        );
    }
}
