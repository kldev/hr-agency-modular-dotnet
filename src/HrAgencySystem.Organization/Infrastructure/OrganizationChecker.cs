using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Port;
using Marten;

namespace HrAgencySystem.Organization.Infrastructure;

public sealed class OrganizationChecker(IQuerySession session)
    : IOrganizationChecker
{
    public async Task<bool> Exists(
        Guid organizationId,
        CancellationToken ct)
    {
        return await session.Query<OrganizationSlugReservation>()
            .Where(z => z.OrganizationId == organizationId).AnyAsync(ct);
    }
}