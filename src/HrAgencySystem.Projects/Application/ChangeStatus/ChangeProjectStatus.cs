using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.ChangeStatus;

public sealed record ChangeProjectStatus(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    ProjectStatus Status,
    string? Reason,
    Guid ModifiedBy
) : IUpdateCommand;
