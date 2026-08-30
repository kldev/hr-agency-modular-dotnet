using HrAgencySystem.SharedKernel.Port;

namespace HrAgencySystem.Organization.Infrastructure;

public sealed class TestOrganizationChecker : IOrganizationChecker
{
    public Task<bool> Exists(
        Guid organizationId,
        CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}