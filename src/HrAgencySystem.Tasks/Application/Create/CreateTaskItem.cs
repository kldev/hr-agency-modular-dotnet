using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Tasks.Domain;

namespace HrAgencySystem.Tasks.Application.Create;

/// <param name="AssigneeId">Whose task it is; the creator's own when left out.</param>
public sealed record CreateTaskItem(
    Guid OrganizationId,
    Guid CreatedBy,
    Guid CompanyId,
    Guid? OpportunityId,
    string Title,
    string? Description,
    DateTimeOffset DueAt,
    TaskPriority Priority,
    Guid? AssigneeId
) : ICreateCommand, ITaskItemInput;
