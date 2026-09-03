using HrAgencySystem.PlatformSeeder.Scenario;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder;

public sealed class HrAgencyShowcaseSeeder(IMessageBus bus) : IPlatformSeeder
{
    public async Task Seed()
    {
        var owner = await new OwnerScenario(bus).Create();
        await SeedAgency(owner.PlatformOwnerId, new SeedConfig());
        await SeedAgency(owner.PlatformOwnerId, new SeedConfig("Flex Jobs", "flex-jobs", 50, 999));
    }

    private async Task SeedAgency(Guid ownerId, SeedConfig config)
    {
        var organizationData = await new OrganizationScenario(bus).Create(ownerId, config.Name, config.Slug);
        var usersIds = await new UserScenario(bus).Create(organizationData, config.UsersCount);
        await Task.Delay(5000);
        var companyIds =
            await new CompanyScenario(bus).Create(organizationData.OrganizationId, usersIds, config.CompaniesCount);
        await Task.Delay(5000);
        await new ProductionJobDescriptionScenario(bus).Create(organizationData.OrganizationId, usersIds, companyIds);
        var javaJobId = await new TechnicalJobDescriptionScenario(bus).Create(organizationData.OrganizationId, usersIds, companyIds);
        
        await new JobPostScenario(bus).Create(javaJobId, organizationData.OrganizationId, usersIds.First());
    }

    private sealed record SeedConfig(
        string Name = "Hr Agency",
        string Slug = "hr-agency",
        int UsersCount = 20,
        int CompaniesCount = 101);
}