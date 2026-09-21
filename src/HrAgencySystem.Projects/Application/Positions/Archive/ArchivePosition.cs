using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Positions.Archive;

public sealed record ArchivePosition(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    Guid ModifiedBy
) : IUpdateCommand;
