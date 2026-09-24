using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Events;

public sealed record FormDefinitionCreated(
    Guid OrganizationId,
    Guid FormId,
    string Code,
    string Name,
    string? Description,
    FormKind Kind,
    ResponseCardinality Cardinality,
    string SubjectKind,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);

/// <summary>Name, description and kind - what a person reads about the form, not its layout.</summary>
public sealed record FormDetailsUpdated(
    Guid OrganizationId,
    Guid FormId,
    string Name,
    string? Description,
    FormKind Kind,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>
/// The whole working layout, as the builder saved it and with every system field filled from the
/// catalogue. Whole rather than per change on purpose - plan 028 §3.3.
/// </summary>
public sealed record FormDraftSaved(
    Guid OrganizationId,
    Guid FormId,
    IReadOnlyList<FormPage> Pages,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>
/// A version goes out. It carries the frozen layout - including its own copy of every system field
/// definition - which is also written, in the same transaction, as the <c>FormVersion</c> document
/// responses are filled against.
/// </summary>
public sealed record FormPublished(
    Guid OrganizationId,
    Guid FormId,
    int Version,
    IReadOnlyList<FormPage> Pages,
    UserSnapshot PublishedBy,
    DateTimeOffset PublishedAt
);

public sealed record FormArchived(
    Guid OrganizationId,
    Guid FormId,
    UserSnapshot ArchivedBy,
    DateTimeOffset ArchivedAt
);
