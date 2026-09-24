using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Tasks.Application.Reopen;

public sealed record ReopenTaskItem([property: Identity] Guid TaskId, Guid OrganizationId, Guid ModifiedBy)
    : IUpdateCommand;
