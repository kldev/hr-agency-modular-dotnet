using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Events;

/// <summary>
/// Somebody opens a response for a person, against the version that is out at that moment. The
/// form's code and name are frozen here so the person's list reads without a lookup per row.
/// <see cref="Prefill"/> is what the system suggested - from the profile or the worker's file - so
/// the history shows what was offered and what a person then typed.
/// </summary>
public sealed record FormResponseStarted(
    Guid OrganizationId,
    Guid ResponseId,
    Guid FormId,
    string FormCode,
    string FormName,
    int FormVersion,
    string SubjectKind,
    Guid SubjectId,
    IReadOnlyList<FieldAnswer> Prefill,
    UserSnapshot StartedBy,
    DateTimeOffset StartedAt
);

/// <summary>The whole current set of answers of an unfinished response.</summary>
public sealed record FormResponseDraftSaved(
    Guid OrganizationId,
    Guid ResponseId,
    IReadOnlyList<FieldAnswer> Answers,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>The response becomes a document: from here on its answers only change by correction.</summary>
public sealed record FormResponseSubmitted(
    Guid OrganizationId,
    Guid ResponseId,
    IReadOnlyList<FieldAnswer> Answers,
    UserSnapshot SubmittedBy,
    DateTimeOffset SubmittedAt
);

/// <summary>
/// A submitted response amended - the full new set of answers and why. The stream of these is the
/// response's revision history; nothing is overwritten that cannot be read back.
/// </summary>
public sealed record FormResponseCorrected(
    Guid OrganizationId,
    Guid ResponseId,
    int Revision,
    IReadOnlyList<FieldAnswer> Answers,
    string Reason,
    UserSnapshot CorrectedBy,
    DateTimeOffset CorrectedAt
);
