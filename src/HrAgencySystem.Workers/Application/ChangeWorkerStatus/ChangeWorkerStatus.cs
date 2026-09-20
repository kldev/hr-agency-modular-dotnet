using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.ChangeWorkerStatus;

public sealed record ChangeWorkerStatus(
    [property: Identity] Guid WorkerId,
    Guid OrganizationId,
    WorkerStatus Status,
    string? Reason,
    Guid ModifiedBy
) : IUpdateCommand;
