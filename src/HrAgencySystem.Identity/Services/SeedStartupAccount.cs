using HrAgencySystem.Identity.Application.Owners.Create;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Microsoft.Extensions.Logging;


namespace HrAgencySystem.Identity.Services;

public class SeedStartupAccount(IDocumentSession session, IClock clock, IPasswordHasher hasher, IOwnerEmailReservationRepository repository, ILogger<SeedStartupAccount> logger) : ISeeder
{
    public async Task SeedAsync(CancellationToken ct)
    {
        var ownerEmail = Environment.GetEnvironmentVariable("HR_AGENCY_EMAIL") ?? "";
        var ownerPassword = Environment.GetEnvironmentVariable("HR_AGENCY_PASSWORD") ?? "";

        if (!string.IsNullOrEmpty(ownerEmail) && !string.IsNullOrEmpty(ownerPassword))
        {
            var exits = await session.Query<PlatformOwnerCreated>().AnyAsync(ct);
            if (exits) return;

            logger.LogInformation($"Creating PlatformOwner: {ownerEmail}");
            var createPlatformOwner = new CreatePlatformOwner(ownerEmail, ownerPassword);
            await CreatePlatformOwnerHandler.Handle(createPlatformOwner, session, clock, hasher, repository, ct);

            await session.SaveChangesAsync(ct);
            logger.LogInformation($"PlatformOwner created");
        }
    }
}