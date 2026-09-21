using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.ClearHead;

public sealed record ClearOrgUnitHead(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    Guid ModifiedBy
);
