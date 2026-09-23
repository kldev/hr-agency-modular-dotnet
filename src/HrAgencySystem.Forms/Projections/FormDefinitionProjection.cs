using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Projections;

/// <summary>
/// One row per form for the list. Deliberately without the layout: the list never shows it, and the
/// builder reads the aggregate itself (see <c>FormsQueryRepository.GetForm</c>), because it must see
/// the draft it has just saved rather than the one a daemon has caught up with.
/// <para>
/// <see cref="Status"/> is flattened so that filtering by it is a column, not a rule.
/// </para>
/// </summary>
public sealed record FormDefinitionProjection(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    FormKind Kind,
    ResponseCardinality Cardinality,
    string SubjectKind,
    FormStatus Status,
    int PublishedVersion,
    bool HasUnpublishedChanges,
    int PageCount,
    int FieldCount,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt,
    DateTimeOffset? PublishedAt
)
{
    public static FormDefinitionProjection Create(FormDefinitionCreated @event) =>
        new(
            @event.FormId,
            @event.OrganizationId,
            @event.Code,
            @event.Name,
            @event.Description,
            @event.Kind,
            @event.Cardinality,
            @event.SubjectKind,
            FormStatus.Draft,
            0,
            false,
            0,
            0,
            @event.CreatedBy,
            @event.CreatedAt,
            null,
            null,
            null
        );

    public FormDefinitionProjection Apply(FormDetailsUpdated @event) =>
        this with
        {
            Name = @event.Name,
            Description = @event.Description,
            Kind = @event.Kind,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    public FormDefinitionProjection Apply(FormDraftSaved @event) =>
        Counted(@event.Pages) with
        {
            HasUnpublishedChanges = true,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    public FormDefinitionProjection Apply(FormPublished @event) =>
        Counted(@event.Pages) with
        {
            Status = Status == FormStatus.Archived ? FormStatus.Archived : FormStatus.Published,
            PublishedVersion = @event.Version,
            HasUnpublishedChanges = false,
            PublishedAt = @event.PublishedAt,
            ModifiedBy = @event.PublishedBy,
            ModifiedAt = @event.PublishedAt,
        };

    public FormDefinitionProjection Apply(FormArchived @event) =>
        this with
        {
            Status = FormStatus.Archived,
            ModifiedBy = @event.ArchivedBy,
            ModifiedAt = @event.ArchivedAt,
        };

    private FormDefinitionProjection Counted(IReadOnlyList<FormPage> pages) =>
        this with { PageCount = pages.Count, FieldCount = pages.AllFields.Count() };
}
