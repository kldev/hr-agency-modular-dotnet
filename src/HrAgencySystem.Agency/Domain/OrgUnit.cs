namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// One box in the company's chart. It knows its parent, so adding "Payroll Germany" under payroll
/// is one row rather than a change of shape.
/// <para>
/// <paramref name="HeadUserId"/> being null is a state, not a gap: "Operations Poland" and
/// "Operations abroad" often have no head of their own, and the head of operations answers for
/// both. That is why the supervisor rule walks up the tree instead of demanding a head per level.
/// </para>
/// </summary>
public sealed record OrgUnit(
    Guid UnitId,
    Guid? ParentId,
    string Name,
    OrgUnitKind Kind,
    Guid? HeadUserId,
    IReadOnlyList<OrgUnitMember> Members,
    bool IsArchived
)
{
    public bool IsRoot => ParentId is null;

    public bool HasMember(Guid userId) => Members.Any(member => member.UserId == userId);
}
