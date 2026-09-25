using HrAgencySystem.Identity.Application.Owners.Create;
using HrAgencySystem.Identity.Events;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class OwnerScenario(IMessageBus bus, IQuerySession session)
{
    private const string OwnerEmail = "admin@hr-agency.com";

    internal async Task<PlatformOwnerCreated> Create()
    {
        // The API may already have created this owner at startup (HR_AGENCY_EMAIL), and a second
        // create would fail the whole seed on "Email already used" - reuse it instead.
        var existing = await session
            .Events.QueryRawEventDataOnly<PlatformOwnerCreated>()
            .Where(owner => owner.Email == OwnerEmail)
            .FirstOrDefaultAsync();
        if (existing is not null)
            return existing;

        var command = new CreatePlatformOwner(OwnerEmail, Config.TestPassword);
        return await bus.InvokeAsync<PlatformOwnerCreated>(command);
    }
}
