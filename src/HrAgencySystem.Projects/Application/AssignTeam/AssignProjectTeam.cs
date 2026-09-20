using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.AssignTeam;

public sealed record AssignProjectTeam(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    Guid TeamId,
    Guid ModifiedBy
) : IUpdateCommand;
