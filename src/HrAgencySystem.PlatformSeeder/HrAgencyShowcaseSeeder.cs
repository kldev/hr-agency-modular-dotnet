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
            await new CompanyScenario(bus).Create(result.OrganizationId);
        }
        catch
        {
            // ignore    
        }

        var other = await new OrganizationScenario(bus).Create("Flex Jobs", "flex-jobs");
        await new CompanyScenario(bus).Create(other.OrganizationId, 999);

    }
}