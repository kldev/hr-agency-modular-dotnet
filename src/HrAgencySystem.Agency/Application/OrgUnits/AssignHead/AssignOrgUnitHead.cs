using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.OrgUnits.AssignHead;

public sealed record AssignOrgUnitHead(
    Guid OrganizationId,
    Guid UnitId,
    Guid HeadUserId,
    Guid ModifiedBy
)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    public Guid Id => OrgStructureId.For(OrganizationId);
}
