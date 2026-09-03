using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal class OrganizationScenario(IMessageBus bus)
{
    public sealed record OrganizationData(Guid OrganizationId, string Slug);

    internal async Task<OrganizationData> Create(Guid ownerId, string name = "HR Agency", string slug = "hr-agency")
    {
        var command = new CreateOrganization(name, slug, ownerId);
        var result = await bus.InvokeAsync<OrganizationCreated>(command);
        
        return new OrganizationData(result.OrganizationId, result.Slug);
    }
}