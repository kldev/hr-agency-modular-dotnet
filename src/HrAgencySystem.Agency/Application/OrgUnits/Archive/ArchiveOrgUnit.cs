using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.Archive;

public sealed record ArchiveOrgUnit(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    Guid ModifiedBy
);
