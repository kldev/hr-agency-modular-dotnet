using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.AssignmentDocuments.Attach;

public sealed record AttachAssignmentDocument(
    [property: Identity] Guid AssignmentId,
    Guid OrganizationId,
    AssignmentDocumentCategory Category,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
