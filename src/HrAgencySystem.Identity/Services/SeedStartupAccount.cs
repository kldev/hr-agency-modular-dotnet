using HrAgencySystem.Identity.Application.Owners.Create;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Identity.Services;

/// <summary>
/// Creates the platform owner from <c>HR_AGENCY_EMAIL</c> / <c>HR_AGENCY_PASSWORD</c> when the API
/// starts, so a stack without the development seed still has somebody who can sign in and create the
/// first organization. Does nothing unless both are set, and nothing once the address is taken - a
/// restart must not fail on "Email already used".
/// </summary>
public sealed class SeedStartupAccount(
    IConfiguration configuration,
    IDocumentSession session,
    IClock clock,
    IPasswordHasher hasher,
    IOwnerEmailReservationRepository repository,
    ILogger<SeedStartupAccount> logger
) : ISeeder
{
    public const string EmailKey = "HR_AGENCY_EMAIL";
    public const string PasswordKey = "HR_AGENCY_PASSWORD";

    public async Task SeedAsync(CancellationToken ct)
    {
        var ownerEmail = configuration[EmailKey];
        var ownerPassword = configuration[PasswordKey];

        if (string.IsNullOrWhiteSpace(ownerEmail) || string.IsNullOrWhiteSpace(ownerPassword))
            return;

        if (await repository.ExistAsync(Email.Create(ownerEmail), ct))
            return;

        var created = await CreatePlatformOwnerHandler.Handle(
            new CreatePlatformOwner(ownerEmail, ownerPassword),
            session,
            clock,
            hasher,
            repository,
            ct
        );
        await session.SaveChangesAsync(ct);

        // The id, not the address: identifiers, not people, in the log.
        logger.LogInformation("Startup platform owner {PlatformOwnerId} created", created.PlatformOwnerId);
    }
}
