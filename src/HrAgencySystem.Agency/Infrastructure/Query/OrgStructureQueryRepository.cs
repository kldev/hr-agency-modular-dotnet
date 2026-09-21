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
        var streamId = OrgStructureId.For(organizationId.Value);

        var projection = await session.LoadAsync<OrgStructureProjection>(streamId, ct);

        if (projection is not null)
            return projection;

        /*
         * The same fallback the project snapshot makes. It matters more here than it looks: a unit
         * is usually created and then filled with people in the same minute, and a supervisor read
         * against a read model that has not caught up would answer "nobody" - which reads exactly
         * like the legitimate answer for the person at the top.
         */
        var structure = await session.Events.AggregateStreamAsync<OrgStructure>(
            streamId,
            token: ct
        );

        if (structure is null)
            return null;

        return new OrgStructureProjection(
            streamId,
            organizationId.Value,
            [
                .. structure.Units.Select(unit => new OrgUnitRow(
                    unit.UnitId,
                    unit.ParentId,
                    unit.Name,
                    unit.Kind,
                    unit.HeadUserId,
                    unit.Members,
                    unit.IsArchived
                )),
            ],
            null,
            null
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
