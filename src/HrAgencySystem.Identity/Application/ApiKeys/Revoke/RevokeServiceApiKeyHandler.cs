using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Identity.Application.ApiKeys.Revoke;

public static class RevokeServiceApiKeyHandler
{
    /// <summary>
    /// Revoking twice is not an error: the key is off either way, and the first revocation - who
    /// and when - is the one worth keeping.
    /// </summary>
    public static async Task<ServiceApiKeyRevoked> Handle(
        RevokeServiceApiKey command,
        IServiceApiKeyRepository keys,
        IClock clock,
        CancellationToken ct
    )
    {
        var key =
            await keys.GetAsync(command.Id, ct)
            ?? throw new NotFoundException("ServiceApiKey", command.Id);

        if (key.IsRevoked)
            return new ServiceApiKeyRevoked(key.Id, key.RevokedAt!.Value);

        var revoked = key.Revoke(command.RevokedBy, clock);

        keys.Update(revoked);

        return new ServiceApiKeyRevoked(revoked.Id, revoked.RevokedAt!.Value);
    }
}
