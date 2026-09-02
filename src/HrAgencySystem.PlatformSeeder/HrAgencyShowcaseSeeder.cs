using HrAgencySystem.PlatformSeeder.Scenario;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder;

public sealed class HrAgencyShowcaseSeeder(IMessageBus bus) : IPlatformSeeder
{
    public  async Task Seed()
    {
        try
        {
            await new OwnerScenario(bus).Create();
            var result = await new OrganizationScenario(bus).Create();
            var companyIds = await new CompanyScenario(bus).Create(result.OrganizationId);
            var usersIds =await new UserScenario(bus).Create(result, 20);

            await Task.Delay(5000);
            await new ProductionJobDescriptionScenario(bus).Create(result.OrganizationId, usersIds, companyIds);
            await new TechnicalJobDescriptionScenario(bus).Create(result.OrganizationId, usersIds, companyIds);
        }
        catch
        {
            // ignore    
        }

        await SeedFlexJobs();


    }

    private async Task SeedFlexJobs()
    {
        var organizationData = await new OrganizationScenario(bus).Create("Flex Jobs", "flex-jobs");
        var companyIds =await new CompanyScenario(bus).Create(organizationData.OrganizationId, 999);
        var usersIds = await new UserScenario(bus).Create(organizationData, 50);
        await Task.Delay(5000);
        await new ProductionJobDescriptionScenario(bus).Create(organizationData.OrganizationId, usersIds, companyIds);
        await new TechnicalJobDescriptionScenario(bus).Create(organizationData.OrganizationId, usersIds, companyIds);
    }
    
}