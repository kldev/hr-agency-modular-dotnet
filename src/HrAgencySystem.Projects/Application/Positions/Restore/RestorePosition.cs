using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Positions.Restore;

public sealed record RestorePosition(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    Guid ModifiedBy
) : IUpdateCommand;
