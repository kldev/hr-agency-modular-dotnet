using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public sealed class FakeOrganizationChecker : IOrganizationChecker
{
    public Task<bool> Exists(
        Guid organizationId,
        CancellationToken ct)
    {
        return Task.FromResult(true);
    }

    public Task<string?> GetSlug(Guid organizationId, CancellationToken ct)
    {
        return Task.FromResult((string?)"hr-agency");
    }

    public Task<OrganizationId> GetOrganizationIdBySlug(string slug, CancellationToken ct)
    {
        return Task.FromResult(OrganizationId.From(Guid.NewGuid()));
    }
}