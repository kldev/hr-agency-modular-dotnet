using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class OwnerScenario(IMessageBus bus)
{
    internal async Task Create()
    {
        var command = new CreatePlatformOwner("admin@hr-agency.com", "pass123");
        await bus.InvokeAsync<PlatformOwnerCreated>(command);
    }
}