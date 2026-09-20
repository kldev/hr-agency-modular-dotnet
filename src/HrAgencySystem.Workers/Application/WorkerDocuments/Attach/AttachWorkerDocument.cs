using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.WorkerDocuments.Attach;

public sealed record AttachWorkerDocument(
    [property: Identity] Guid WorkerId,
    Guid OrganizationId,
    WorkerDocumentCategory Category,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
