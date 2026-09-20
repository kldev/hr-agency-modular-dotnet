using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Documents.UpdateMetadata;

public sealed record UpdateProjectDocumentMetadata(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    Guid DocumentId,
    DocumentCategory Category,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
