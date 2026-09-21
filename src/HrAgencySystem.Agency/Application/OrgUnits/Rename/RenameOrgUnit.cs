using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Agency.Application.OrgUnits.Rename;

public sealed record RenameOrgUnit(
    [property: Identity] Guid OrganizationId,
    Guid UnitId,
    string Name,
    Guid ModifiedBy
);
