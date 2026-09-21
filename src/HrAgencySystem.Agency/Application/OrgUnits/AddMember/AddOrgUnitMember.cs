using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.AddMember;

public sealed record AddOrgUnitMember(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    Guid UserId,
    string? Title,
    Guid ModifiedBy
);
