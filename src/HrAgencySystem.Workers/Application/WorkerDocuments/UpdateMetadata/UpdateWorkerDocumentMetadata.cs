using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.WorkerDocuments.UpdateMetadata;

public sealed record UpdateWorkerDocumentMetadata(
    [property: Identity] Guid WorkerId,
    Guid OrganizationId,
    Guid DocumentId,
    WorkerDocumentCategory Category,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
