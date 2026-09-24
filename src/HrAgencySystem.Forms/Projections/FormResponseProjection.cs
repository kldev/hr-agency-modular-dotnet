using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Projections;

/// <summary>
/// One row per response: what a person's "Forms" tab lists, and what a search by answer runs over.
/// <para>
/// <see cref="Answers"/> is keyed by field code on purpose. Stored as JSONB under a GIN index, "who
/// answered <c>gdpr.consent = true</c>" is a containment query (<c>data @> …</c>) the index serves,
/// with no table per form and no migration per new document. This is the MVP half of the storage
/// decision in plan 028 §C; the relational <c>reports.form_answers</c> is the reporting half, later.
/// </para>
/// </summary>
public sealed record FormResponseProjection(
    Guid Id,
    Guid OrganizationId,
    Guid FormId,
    string FormCode,
    string FormName,
    int FormVersion,
    string SubjectKind,
    Guid SubjectId,
    FormResponseStatus Status,
    int Revision,
    Dictionary<string, FieldValue> Answers,
    UserSnapshot StartedBy,
    DateTimeOffset StartedAt,
    UserSnapshot? SubmittedBy,
    DateTimeOffset? SubmittedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    public static FormResponseProjection Create(FormResponseStarted @event) =>
        new(
            @event.ResponseId,
            @event.OrganizationId,
            @event.FormId,
            @event.FormCode,
            @event.FormName,
            @event.FormVersion,
            @event.SubjectKind,
            @event.SubjectId,
            FormResponseStatus.Draft,
            0,
            Keyed(@event.Prefill),
            @event.StartedBy,
            @event.StartedAt,
            null,
            null,
            @event.StartedBy,
            @event.StartedAt
        );

    public FormResponseProjection Apply(FormResponseDraftSaved @event) =>
        this with
        {
            Answers = Keyed(@event.Answers),
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    public FormResponseProjection Apply(FormResponseSubmitted @event) =>
        this with
        {
            Answers = Keyed(@event.Answers),
            Status = FormResponseStatus.Submitted,
            SubmittedBy = @event.SubmittedBy,
            SubmittedAt = @event.SubmittedAt,
            ModifiedBy = @event.SubmittedBy,
            ModifiedAt = @event.SubmittedAt,
        };

    public FormResponseProjection Apply(FormResponseCorrected @event) =>
        this with
        {
            Answers = Keyed(@event.Answers),
            Revision = @event.Revision,
            ModifiedBy = @event.CorrectedBy,
            ModifiedAt = @event.CorrectedAt,
        };

    public static Dictionary<string, FieldValue> Keyed(IEnumerable<FieldAnswer> answers) =>
        answers.ToDictionary(answer => answer.FieldCode, answer => answer.Value);
}
