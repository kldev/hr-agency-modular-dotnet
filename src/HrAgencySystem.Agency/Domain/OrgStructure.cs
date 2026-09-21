using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// The company's chart, whole, as one aggregate per organization - the stream id is the
/// organization id.
/// <para>
/// Not one aggregate per unit, and that is the central decision here. Every rule worth having
/// spans more than one box: there is exactly one root, a unit may not be moved under its own
/// descendant, and a person belongs to one unit. Split per unit, each of those becomes a question
/// asked of a read model that the async daemon has not caught up with yet - and a cycle admitted
/// once makes the supervisor walk loop forever. Whole, they are plain checks on a list in memory,
/// settled in the transaction that writes the change.
/// </para>
/// <para>
/// It is affordable because a chart is small: an agency has tens of units, not thousands, and the
/// shape changes a few times a year.
/// </para>
/// </summary>
public sealed class OrgStructure : IOrganizationDomain
{
    private List<OrgUnit> _units = [];

    private OrgStructure() { }

    public static OrgStructure Empty() => new();

    public OrganizationId OrganizationId { get; private set; }

    public IReadOnlyList<OrgUnit> Units => _units;

    /// <summary>Units that still take people; the archived ones stay readable but are not offered.</summary>
    public IReadOnlyList<OrgUnit> ActiveUnits => [.. _units.Where(unit => !unit.IsArchived)];

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ModifiedAt { get; private set; }
    public Guid? ModifiedById { get; private set; }

    public OrgUnit? UnitById(Guid unitId) => _units.FirstOrDefault(unit => unit.UnitId == unitId);

    public OrgUnit? Root => _units.FirstOrDefault(unit => unit.IsRoot);

    /// <summary>Which unit somebody sits in. One, or none - never two; see <see cref="UnitOfAnother"/>.</summary>
    public OrgUnit? UnitOf(Guid userId) => _units.FirstOrDefault(unit => unit.HasMember(userId));

    /// <summary>The unit this person is already in, when it is not the one being written to.</summary>
    public OrgUnit? UnitOfAnother(Guid userId, Guid exceptUnitId) =>
        _units.FirstOrDefault(unit => unit.UnitId != exceptUnitId && unit.HasMember(userId));

    /// <summary>
    /// Whether a sibling already carries this name. Scoped to one parent on purpose: two companies
    /// can both have a "Payroll", and under one parent two of them cannot be told apart.
    /// </summary>
    public bool HasSiblingNamed(Guid? parentId, string name, Guid? exceptUnitId = null) =>
        _units.Any(unit =>
            unit.ParentId == parentId
            && unit.UnitId != exceptUnitId
            && !unit.IsArchived
            && string.Equals(unit.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)
        );

    public IReadOnlyList<OrgUnit> ChildrenOf(Guid unitId) =>
        [.. _units.Where(unit => unit.ParentId == unitId && !unit.IsArchived)];

    public void Apply(OrgUnitCreated @event)
    {
        OrganizationId = OrganizationId.From(@event.OrganizationId);

        _units =
        [
            .. _units,
            new OrgUnit(@event.UnitId, @event.ParentId, @event.Name, @event.Kind, null, [], false),
        ];

        if (CreatedAt == default)
            CreatedAt = @event.CreatedAt;

        Touch(@event.CreatedBy, @event.CreatedAt);
    }

    public void Apply(OrgUnitRenamed @event)
    {
        Replace(@event.UnitId, unit => unit with { Name = @event.Name });
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(OrgUnitMoved @event)
    {
        Replace(@event.UnitId, unit => unit with { ParentId = @event.ParentId });
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(OrgUnitHeadAssigned @event)
    {
        Replace(@event.UnitId, unit => unit with { HeadUserId = @event.HeadUserId });
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(OrgUnitHeadCleared @event)
    {
        Replace(@event.UnitId, unit => unit with { HeadUserId = null });
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(OrgUnitMemberAdded @event)
    {
        Replace(@event.UnitId, unit => unit with { Members = [.. unit.Members, @event.Member] });
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(OrgUnitMemberRemoved @event)
    {
        Replace(
            @event.UnitId,
            unit =>
                unit with
                {
                    Members = [.. unit.Members.Where(member => member.UserId != @event.UserId)],
                }
        );

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(OrgUnitArchived @event)
    {
        Replace(@event.UnitId, unit => unit with { IsArchived = true });
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    private void Replace(Guid unitId, Func<OrgUnit, OrgUnit> change) =>
        _units = [.. _units.Select(unit => unit.UnitId == unitId ? change(unit) : unit)];

    private void Touch(UserSnapshot by, DateTimeOffset at)
    {
        ModifiedAt = at;
        ModifiedById = by.Id;
    }
}
