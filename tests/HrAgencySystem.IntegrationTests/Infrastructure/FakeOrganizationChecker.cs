using HrAgencySystem.SharedKernel.Port;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public sealed class FakeOrganizationChecker : IOrganizationChecker
{
    public Task<bool> Exists(
        Guid organizationId,
        CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}