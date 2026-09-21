using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Agency.Projections;

/// <summary>
/// The chart as one document per organization, keyed by the organization id - the same shape as
/// the aggregate, because the aggregate is the whole chart.
/// <para>
/// One document rather than one per unit, and that is not laziness: every question anybody asks of
/// this ("who is above me", "who reports to me, all the way down", "show me the tree") needs
/// ancestors or descendants, and Marten cannot walk a parent chain across documents. Held whole,
/// each of those is a walk over a list of tens of rows, which is why the plan's stored
/// <c>Path</c> column is not here - it was buying a join that no longer has to happen.
/// </para>
/// </summary>
public sealed record OrgStructureProjection(
    Guid Id,
    Guid OrganizationId,
    IReadOnlyList<OrgUnitRow> Units,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    public int UnitCount => Units.Count(unit => !unit.IsArchived);

    public static OrgStructureProjection Create(OrgUnitCreated @event)
    {
        var empty = new OrgStructureProjection(
            OrgStructureId.For(@event.OrganizationId),
            @event.OrganizationId,
            [],
            @event.CreatedBy,
            @event.CreatedAt
        );

        return empty.Apply(@event);
    }

    public OrgStructureProjection Apply(OrgUnitCreated @event) =>
        (
            this with
            {
                Units =
                [
                    .. Units,
                    new OrgUnitRow(
                        @event.UnitId,
                        @event.ParentId,
                        @event.Name,
                        @event.Kind,
                        null,
                        [],
                        false
                    ),
                ],
            }
        ).Touched(@event.CreatedBy, @event.CreatedAt);

    public OrgStructureProjection Apply(OrgUnitRenamed @event) =>
        Replace(@event.UnitId, unit => unit with { Name = @event.Name })
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public OrgStructureProjection Apply(OrgUnitMoved @event) =>
        Replace(@event.UnitId, unit => unit with { ParentId = @event.ParentId })
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public OrgStructureProjection Apply(OrgUnitHeadAssigned @event) =>
        Replace(@event.UnitId, unit => unit with { HeadUserId = @event.HeadUserId })
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public OrgStructureProjection Apply(OrgUnitHeadCleared @event) =>
        Replace(@event.UnitId, unit => unit with { HeadUserId = null })
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public OrgStructureProjection Apply(OrgUnitMemberAdded @event) =>
        Replace(@event.UnitId, unit => unit with { Members = [.. unit.Members, @event.Member] })
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public OrgStructureProjection Apply(OrgUnitMemberRemoved @event) =>
        Replace(
                @event.UnitId,
                unit =>
                    unit with
                    {
                        Members = [.. unit.Members.Where(m => m.UserId != @event.UserId)],
                    }
            )
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public OrgStructureProjection Apply(OrgUnitArchived @event) =>
        Replace(@event.UnitId, unit => unit with { IsArchived = true })
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    /// <summary>The rows as the domain sees them, so the supervisor rule runs on one shape only.</summary>
    public IReadOnlyList<OrgUnit> AsUnits() =>
        [
            .. Units.Select(unit => new OrgUnit(
                unit.UnitId,
                unit.ParentId,
                unit.Name,
                unit.Kind,
                unit.HeadUserId,
                unit.Members,
                unit.IsArchived
            )),
        ];

    private OrgStructureProjection Replace(Guid unitId, Func<OrgUnitRow, OrgUnitRow> change) =>
        this with
        {
            Units = [.. Units.Select(unit => unit.UnitId == unitId ? change(unit) : unit)],
        };

    private OrgStructureProjection Touched(UserSnapshot by, DateTimeOffset at) =>
        this with
        {
            ModifiedBy = by,
            ModifiedAt = at,
        };
}

public sealed record OrgUnitRow(
    Guid UnitId,
    Guid? ParentId,
    string Name,
    OrgUnitKind Kind,
    Guid? HeadUserId,
    IReadOnlyList<OrgUnitMember> Members,
    bool IsArchived
);
