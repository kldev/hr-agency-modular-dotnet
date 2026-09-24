using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Tasks.Application.Complete;

public sealed record CompleteTaskItem([property: Identity] Guid TaskId, Guid OrganizationId, Guid ModifiedBy)
    : IUpdateCommand;
