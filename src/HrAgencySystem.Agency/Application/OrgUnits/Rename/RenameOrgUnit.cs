using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.OrgUnits.Rename;

public sealed record RenameOrgUnit(Guid OrganizationId, Guid UnitId, string Name, Guid ModifiedBy)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    public Guid Id => OrgStructureId.For(OrganizationId);
}
