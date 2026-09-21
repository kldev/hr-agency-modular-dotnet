using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// A role opens inside the project. The whole position travels in the event rather than a handful
/// of fields, for the same reason <see cref="ProjectDocumentAttached"/> carries the document: the
/// aggregate and the projection then agree on the shape without either of them assembling it.
/// </summary>
public sealed record ProjectPositionOpened(
    Guid ProjectId,
    Guid OrganizationId,
    ProjectPosition Position,
    UserSnapshot OpenedBy,
    DateTimeOffset OpenedAt
);
