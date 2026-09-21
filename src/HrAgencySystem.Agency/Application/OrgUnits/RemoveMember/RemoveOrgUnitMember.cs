using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.RemoveMember;

public sealed record RemoveOrgUnitMember(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    Guid UserId,
    Guid ModifiedBy
);
