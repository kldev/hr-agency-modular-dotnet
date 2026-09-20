using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Documents.Attach;

/// <summary>
/// The bytes are already stored by the time this command exists: it carries a <c>FileId</c> and
/// nothing else about storage. A command travels through the outbox and cannot hold a stream.
/// </summary>
public sealed record AttachProjectDocument(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    DocumentCategory Category,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
