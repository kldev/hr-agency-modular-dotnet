using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// Writes the job board's key straight into the store instead of issuing it. Issuing draws a random
/// value, and the whole point here is a value known in advance - see
/// <see cref="Config.JobBoardApiKey"/>. Stored the way an issued key is: the hash and the display
/// prefix, never the value.
/// </summary>
internal sealed class ServiceApiKeyScenario(IDocumentSession session)
{
    /// <summary>Fixed too, so reseeding recognises the key it wrote last time.</summary>
    private static readonly Guid KeyId = Guid.Parse("5eed0000-0000-4000-8000-00000000a91e");

    internal async Task Create(Guid ownerId)
    {
        var hash = SecureToken.Hash(Config.JobBoardApiKey);

        if (await session.Query<ServiceApiKey>().AnyAsync(key => key.KeyHash == hash))
            return;

        session.Insert(
            new ServiceApiKey(
                KeyId,
                "public job board (seeded)",
                hash,
                Config.JobBoardApiKey[..11],
                DateTimeOffset.UtcNow,
                ownerId
            )
        );

        await session.SaveChangesAsync();
    }
}
