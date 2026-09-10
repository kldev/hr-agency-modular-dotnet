using HrAgencySystem.Identity.Application.Owners.Create;
using HrAgencySystem.Identity.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class OwnerScenario(IMessageBus bus)
{
    internal async Task<PlatformOwnerCreated> Create()
    {
        var command = new CreatePlatformOwner("admin@hr-agency.com", Config.TestPassword);
        return await bus.InvokeAsync<PlatformOwnerCreated>(command);
    }
}