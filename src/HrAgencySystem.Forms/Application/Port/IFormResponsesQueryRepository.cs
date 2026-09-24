using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Forms.Application.Port;

/// <summary>
/// One response as it is opened: its answers, the frozen version it was given to - the renderer needs
/// both, and there is no reason for two requests - and its history.
/// </summary>
public sealed record FormResponseView(
    Guid Id,
    Guid FormId,
    int FormVersion,
    string SubjectKind,
    Guid SubjectId,
    FormResponseStatus Status,
    int Revision,
    IReadOnlyList<FieldAnswer> Answers,
    FormVersion Version,
    IReadOnlyList<FormResponseHistoryEntry> History,
    UserSnapshot StartedBy,
    DateTimeOffset StartedAt,
    UserSnapshot? SubmittedBy,
    DateTimeOffset? SubmittedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
);

public enum FormResponseHistoryKind
{
    Started,
    DraftSaved,
    Submitted,
    Corrected,
}

/// <summary>One line of a response's history: who, when, and - for a correction - why.</summary>
public sealed record FormResponseHistoryEntry(
    FormResponseHistoryKind Kind,
    int Revision,
    UserSnapshot By,
    DateTimeOffset At,
    string? Reason
);

/// <summary>"Who answered X" - one field, one value, optionally only submitted responses.</summary>
public sealed record FormAnswerQuery(
    string FieldCode,
    FieldValue Value,
    FormResponseStatus? Status,
    int Page,
    int PageSize
) : IPagedQuery;

public interface IFormResponsesQueryRepository
{
    Task<IReadOnlyList<FormResponseProjection>> GetForSubject(
        OrganizationId organizationId,
        SubjectRef subject,
        CancellationToken ct
    );

    /// <summary>Replayed, so a response opened right after "Next" shows what was just saved.</summary>
    Task<FormResponseView?> GetResponse(OrganizationId organizationId, Guid responseId, CancellationToken ct);

    /// <summary>
    /// A containment query on the answers - <c>data @> {"Answers": {code: value}}</c> - served by the
    /// GIN index on the projection. A selection matches when it contains every value asked for.
    /// </summary>
    Task<SliceResponse<FormResponseProjection>> FindByAnswer(
        OrganizationId organizationId,
        Guid formId,
        FormAnswerQuery query,
        CancellationToken ct
    );
}
