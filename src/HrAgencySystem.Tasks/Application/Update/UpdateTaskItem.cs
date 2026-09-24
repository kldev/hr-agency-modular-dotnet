using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Tasks.Domain;
using JasperFx;

namespace HrAgencySystem.Tasks.Application.Update;

public sealed record UpdateTaskItem(
    [property: Identity] Guid TaskId,
    Guid OrganizationId,
    Guid ModifiedBy,
    Guid? OpportunityId,
    string Title,
    string? Description,
    DateTimeOffset DueAt,
    TaskPriority Priority,
    Guid AssigneeId
) : IUpdateCommand, ITaskItemInput;
