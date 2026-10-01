using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Agency.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
// Public, not internal: Wolverine generates handler code into another assembly and falls back to
// service location on an internal type, which throws at runtime.
public sealed class OrgStructureQueryRepository(
    IDocumentSession session,
    IUserSnapshotRepository users
) : IOrgStructureQueryRepository
{
    public async Task<OrgStructureProjection?> GetStructureAsync(
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        /*
         * FetchLatest, not LoadAsync: the snapshot is async, so the stored document can lag behind
         * the chart. A unit is usually created and then filled with people in the same minute, and
         * a supervisor read against a read model that has not caught up would answer "nobody" -
         * which reads exactly like the legitimate answer for the person at the top. Falling back to
         * the stream only when the document was missing covered the first event and nothing after
         * it; FetchLatest applies every event the daemon has not processed yet.
         */
        return await session.Events.FetchLatest<OrgStructureProjection>(
            OrgStructureId.For(organizationId.Value),
            ct
        );
    }

    public async Task<SupervisorView?> GetSupervisorAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    )
    {
        var structure = await GetStructureAsync(organizationId, ct);

        if (structure is null)
            return null;

        var units = structure.AsUnits();
        var supervisorId = SupervisorPolicy.SupervisorOf(units, userId);

        if (supervisorId is null)
            return null;

        var unit = units.First(candidate => candidate.HeadUserId == supervisorId);
        var user = await users.GetUserAsync(supervisorId.Value, organizationId, ct);

        return user is null
            ? null
            : new SupervisorView(user.Id, user.Fullname, user.Email, unit.UnitId, unit.Name);
    }

    public async Task<IReadOnlyList<Guid>> GetSubordinatesAsync(
        OrganizationId organizationId,
        Guid userId,
        bool wholeSubtree,
        CancellationToken ct
    )
    {
        var structure = await GetStructureAsync(organizationId, ct);

        return structure is null
            ? []
            : SupervisorPolicy.SubordinatesOf(structure.AsUnits(), userId, wholeSubtree);
    }
}
