using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.OrgUnits.AddMember;

public sealed record AddOrgUnitMember(
    Guid OrganizationId,
    Guid UnitId,
    Guid UserId,
    string? Title,
    Guid ModifiedBy
)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    public Guid Id => OrgStructureId.For(OrganizationId);
}
