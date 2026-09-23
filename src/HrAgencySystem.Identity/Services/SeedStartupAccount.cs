using HrAgencySystem.Identity.Application.Owners.Create;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Port;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.Identity.Services;

public class SeedStartupAccount(IMessageBus bus, IQuerySession session, ILogger<SeedStartupAccount> logger) : ISeeder
{
    public async Task SeedAsync(CancellationToken ct)
    {
        var ownerEmail = Environment.GetEnvironmentVariable("HR_AGENCY_EMAIL") ?? "";
        var ownerPassword = Environment.GetEnvironmentVariable("HR_AGENCY_PASSWORD")??"";

        if (!string.IsNullOrEmpty(ownerEmail) && !string.IsNullOrEmpty(ownerPassword))
        {
            var exits = await session.Query<PlatformOwnerCreated>().AnyAsync(ct);
            if (exits) return;

            logger.LogInformation($"Creating PlatformOwner: {ownerEmail}");
            var createPlatformOwner = new CreatePlatformOwner(ownerEmail, ownerPassword);
            var result = await bus.InvokeAsync<PlatformOwnerCreated>(createPlatformOwner, ct);
            logger.LogInformation($"Created PlatformOwner: {result.PlatformOwnerId}");
        }
        
    }
}