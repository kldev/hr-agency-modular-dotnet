using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Teams.Application.Rename;

public sealed record RenameTeam(
    [property: Identity] Guid TeamId,
    Guid OrganizationId,
    string Name,
    Guid ModifiedBy
) : IUpdateCommand;
