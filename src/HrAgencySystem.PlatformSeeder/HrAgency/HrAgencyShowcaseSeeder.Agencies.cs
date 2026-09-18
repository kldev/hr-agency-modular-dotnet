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

        await GenerateApplicants(20);

        logger.LogInformation(
            "Generated applicants for organization {OrganizationId}",
            organization.OrganizationId
        );

        await WaitForProjections();

        logger.LogInformation(
            "Creating interviews for organization {OrganizationId}",
            organization.OrganizationId
        );

        await new InterviewsScenario(bus, session).SeedAsync(organization.OrganizationId, 200);

        logger.LogInformation(
            "Agency seed completed: {Slug} ({OrganizationId})",
            organization.Slug,
            organization.OrganizationId
        );

        await new TagsScenario(bus, session).Seed(organization.OrganizationId, userIds);
    }

    private async Task SeedMinimalAgency(Guid ownerId, SeedConfig config)
    {
        var organization = await new OrganizationScenario(bus).Create(
            ownerId,
            config.Name,
            config.Slug
        );

        var userIds = await CreateUsers(config, organization);

        var companyIds = await CreateCompanies(config, organization, userIds);

        await CreateModernDeveloperPosts(organization, userIds, companyIds);

        await PostToChannel(userIds);

        await GenerateApplicants(20);

        await new TagsScenario(bus, session).Seed(organization.OrganizationId, userIds);
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
