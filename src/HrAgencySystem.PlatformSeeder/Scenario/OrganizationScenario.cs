using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal class OrganizationScenario(IMessageBus bus)
{
    public sealed record OrganizationData(Guid OrganizationId);

    internal async Task<OrganizationData> Create(string name = "HR Agency", string slug = "hr-agency")
    {
        Guid? id = slug == "hr-agency" ? Guid.Parse("1ea044bf-48cc-4ed3-9174-cc0f5b8a0583") : null;
        var command = new CreateOrganization(name, slug, id);
        var result = await bus.InvokeAsync<OrganizationCreated>(command);
        return new OrganizationData(result.OrganizationId);
    }
}