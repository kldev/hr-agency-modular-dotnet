using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.AssignmentDocuments.UpdateMetadata;

public sealed record UpdateAssignmentDocumentMetadata(
    [property: Identity] Guid AssignmentId,
    Guid OrganizationId,
    Guid DocumentId,
    AssignmentDocumentCategory Category,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
