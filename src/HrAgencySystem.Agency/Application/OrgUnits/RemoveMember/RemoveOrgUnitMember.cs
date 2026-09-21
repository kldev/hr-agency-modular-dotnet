using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.OrgUnits.RemoveMember;

public sealed record RemoveOrgUnitMember(
    Guid OrganizationId,
    Guid UnitId,
    Guid UserId,
    Guid ModifiedBy
)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    public Guid Id => OrgStructureId.For(OrganizationId);
}
