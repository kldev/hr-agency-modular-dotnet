using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.AssignHead;

public sealed record AssignOrgUnitHead(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    Guid HeadUserId,
    Guid ModifiedBy
);
