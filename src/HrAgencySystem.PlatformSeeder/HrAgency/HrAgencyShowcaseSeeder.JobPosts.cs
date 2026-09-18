using HrAgencySystem.PlatformSeeder.Scenario;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

public sealed partial class HrAgencyShowcaseSeeder
{
    private async Task CreateProductionJobDescription(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds
    )
    {
        logger.LogDebug(
            "Creating production job description for organization {OrganizationId}",
            organization.OrganizationId
        );

        await new ProductionJobDescriptionScenario(bus).Create(
            organization.OrganizationId,
            userIds,
            companyIds
        );
    }

    private async Task<Guid> CreateTechnicalJobsDescription(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds
    )
    {
        logger.LogDebug(
            "Creating technical job description for organization {OrganizationId}",
            organization.OrganizationId
        );

        return await new TechnicalJobDescriptionScenario(bus).Create(
            organization.OrganizationId,
            userIds,
            companyIds
        );
    }

    private async Task CreateMoreJobPosting(
        Guid javaJobDescriptionId,
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds
    )
    {
        logger.LogDebug(
            "Creating additional job post for job description {JobDescriptionId}",
            javaJobDescriptionId
        );

        await new JobPostScenario(bus).Create(
            javaJobDescriptionId,
            organization.OrganizationId,
            userIds.First()
        );
    }

    private async Task CreateModernDeveloperPosts(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds
    )
    {
        logger.LogDebug(
            "Creating modern developer job posts for organization {OrganizationId}",
            organization.OrganizationId
        );

        await new ModernWebDeveloperScenario(bus).Create(
            organization.OrganizationId,
            userIds,
            companyIds
        );
    }
}
