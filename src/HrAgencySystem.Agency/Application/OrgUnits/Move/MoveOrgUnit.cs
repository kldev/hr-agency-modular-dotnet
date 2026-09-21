using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.OrgUnits.Move;

public sealed record MoveOrgUnit(Guid OrganizationId, Guid UnitId, Guid ParentId, Guid ModifiedBy)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    public Guid Id => OrgStructureId.For(OrganizationId);
}
