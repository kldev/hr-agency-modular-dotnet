using HrAgencySystem.Identity.Infrastructure.Persistence;

namespace HrAgencySystem.Identity.Application.Port;

public interface IServiceApiKeyRepository
{
    /// <summary>The key a presented value belongs to, revoked or not - the caller decides.</summary>
    Task<ServiceApiKey?> FindByHashAsync(string keyHash, CancellationToken ct);

    Task<ServiceApiKey?> GetAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<ServiceApiKey>> ListAsync(CancellationToken ct);

    void Issue(ServiceApiKey key);

    void Update(ServiceApiKey key);
}
