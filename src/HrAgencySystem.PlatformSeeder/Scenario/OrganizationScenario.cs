using HrAgencySystem.Organization.Application.Create;
using HrAgencySystem.Organization.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal class OrganizationScenario(IMessageBus bus)
{
    public sealed record OrganizationData(Guid OrganizationId, string Slug);

    internal async Task<OrganizationData> Create(Guid ownerId, string name = "HR Agency", string slug = "hr-agency")
    {
        var emailDomains = new List<string>() { slug + ".com", slug + ".com.pl", slug + ".eu"  };
        var command = new CreateOrganization(name, slug, ownerId, emailDomains);
        var result = await bus.InvokeAsync<OrganizationCreated>(command);
        
        return new OrganizationData(result.OrganizationId, result.Slug);
    }
}