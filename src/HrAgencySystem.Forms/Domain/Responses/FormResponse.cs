using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Forms.Domain.Responses;

/// <summary>
/// One filling-in of one version of a form for one person.
/// <para>
/// Bound to its version for life. A correction made after v3 is out still corrects a v1 response
/// against v1's layout: the version is the structure the answers were given to, and moving them to
/// another would be rewriting what somebody declared.
/// </para>
/// <para>
/// The stream is the history: every draft save, the submission and each correction with its reason.
/// No separate audit log is needed to answer "who changed this consent and why".
/// </para>
/// </summary>
public sealed class FormResponse : IOrganizationDomain
{
    private List<FieldAnswer>? _answers = [];

    private FormResponse() { }

    public static FormResponse Empty() => new();

    public Guid Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }

    public Guid FormId { get; private set; }
    public int FormVersion { get; private set; }
    public SubjectRef Subject { get; private set; } = null!;

    public FormResponseStatus Status { get; private set; }

    /// <summary>Zero until the first correction.</summary>
    public int Revision { get; private set; }

    public IReadOnlyList<FieldAnswer> Answers => _answers ?? [];

    public UserSnapshot StartedBy { get; private set; } = null!;
    public DateTimeOffset StartedAt { get; private set; }
    public UserSnapshot? SubmittedBy { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public UserSnapshot? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    public void Apply(FormResponseStarted @event)
    {
        Id = @event.ResponseId;
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        FormId = @event.FormId;
        FormVersion = @event.FormVersion;
        Subject = new SubjectRef(@event.SubjectKind, @event.SubjectId);
        Status = FormResponseStatus.Draft;
        _answers = [.. @event.Prefill];
        StartedBy = @event.StartedBy;
        StartedAt = @event.StartedAt;
    }

    public void Apply(FormResponseDraftSaved @event)
    {
        _answers = [.. @event.Answers];
        ModifiedBy = @event.ModifiedBy;
        ModifiedAt = @event.ModifiedAt;
    }

    public void Apply(FormResponseSubmitted @event)
    {
        _answers = [.. @event.Answers];
        Status = FormResponseStatus.Submitted;
        SubmittedBy = @event.SubmittedBy;
        SubmittedAt = @event.SubmittedAt;
        ModifiedBy = @event.SubmittedBy;
        ModifiedAt = @event.SubmittedAt;
    }

    public void Apply(FormResponseCorrected @event)
    {
        _answers = [.. @event.Answers];
        Revision = @event.Revision;
        ModifiedBy = @event.CorrectedBy;
        ModifiedAt = @event.CorrectedAt;
    }
}
