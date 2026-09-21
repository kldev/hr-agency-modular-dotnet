using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.OrgUnits.Create;

/// <summary>
/// Opens a box in the chart. <paramref name="ParentId"/> null makes it the root, of which there is
/// exactly one - the board.
/// </summary>
public sealed record CreateOrgUnit(
    Guid OrganizationId,
    Guid? ParentId,
    string Name,
    OrgUnitKind Kind,
    Guid CreatedBy
);
