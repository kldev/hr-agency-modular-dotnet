using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.Move;

public sealed record MoveOrgUnit(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    Guid ParentId,
    Guid ModifiedBy
);
