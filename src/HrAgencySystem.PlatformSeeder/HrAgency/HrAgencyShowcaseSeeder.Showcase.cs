using HrAgencySystem.Company.Events;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.PlatformSeeder.Scenario;
using Marten;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

public sealed partial class HrAgencyShowcaseSeeder
{
    public async Task SeedApplicants(int count)
    {
        logger.LogInformation(
            "Starting applicant seeding: {Count} applicants per organization",
            count
        );

        var organizations = await session.Query<OrganizationCreated>().ToListAsync();

        logger.LogInformation("Found {OrganizationCount} organizations", organizations.Count);

        foreach (var organization in organizations)
        {
            logger.LogInformation(
                "Generating {Count} applicants for organization {OrganizationId}",
                count,
                organization.OrganizationId
            );

            await GenerateApplicants(count);
        }

        logger.LogInformation("Applicant seeding completed");
    }

    public async Task SeedShowcase()
    {
        logger.LogInformation("Starting showcase seeding");

        var organizations = await session.Query<OrganizationCreated>().ToListAsync();

        logger.LogInformation(
            "Found {OrganizationCount} organizations for showcase",
            organizations.Count
        );

        foreach (var organization in organizations)
        {
            logger.LogInformation(
                "Creating showcase data for organization {OrganizationId}",
                organization.OrganizationId
            );

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
                companyIds.Count
            );

            await new ModernWebDeveloperScenario(bus).Create(
                organization.OrganizationId,
                userIds,
                companyIds
            );
        }

        logger.LogInformation("Creating showcase job applications");

        await new ApplyToJobPostScenario(bus, session).ExecuteShowcase();

        logger.LogInformation("Showcase seeding completed");
    }
}
