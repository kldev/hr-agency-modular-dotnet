using HrAgencySystem.Identity.Application.Port;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

// Public, not internal: Wolverine generates handler code into another assembly and falls back to
// service location on an internal type, which throws at runtime.
public sealed class ServiceApiKeyRepository(IDocumentSession session) : IServiceApiKeyRepository
{
    public async Task<ServiceApiKey?> FindByHashAsync(string keyHash, CancellationToken ct) =>
        await session.Query<ServiceApiKey>().SingleOrDefaultAsync(z => z.KeyHash == keyHash, ct);

    public Task<ServiceApiKey?> GetAsync(Guid id, CancellationToken ct) =>
        session.LoadAsync<ServiceApiKey>(id, ct);

    public async Task<IReadOnlyList<ServiceApiKey>> ListAsync(CancellationToken ct) =>
        await session.Query<ServiceApiKey>().OrderByDescending(z => z.CreatedAt).ToListAsync(ct);

    public void Issue(ServiceApiKey key) => session.Insert(key);

    public void Update(ServiceApiKey key) => session.Update(key);
}
