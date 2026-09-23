using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Forms.Domain;

/// <summary>
/// A form: its identity, one working draft of its layout, and how many versions of it are out.
/// <para>
/// The draft may be edited at any time, published or not - editing touches the draft alone.
/// Publishing freezes a copy of it as the next version, which is what people then fill in; a
/// response never follows the draft, so an edit after v1 cannot make a response to v1 wrong.
/// </para>
/// <para>
/// No decisions here: <c>Apply</c> replays what happened. The rules live in the handlers and in
/// <see cref="FormLayoutPolicy"/>.
/// </para>
/// </summary>
public sealed class FormDefinition : IOrganizationDomain
{
    // Marten rebuilds an aggregate without running field initialisers - read through the property.
    private List<FormPage>? _pages = [];

    private FormDefinition() { }

    public static FormDefinition Empty() => new();

    public Guid Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }

    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public FormKind Kind { get; private set; }
    public ResponseCardinality Cardinality { get; private set; }
    public string SubjectKind { get; private set; } = "";

    public IReadOnlyList<FormPage> Pages => _pages ?? [];

    /// <summary>The number of the last version out; zero while it never was.</summary>
    public int PublishedVersion { get; private set; }

    public bool IsArchived { get; private set; }

    /// <summary>Whether the draft moved on since the last publication.</summary>
    public bool HasUnpublishedChanges { get; private set; }

    public UserSnapshot CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public UserSnapshot? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    public FormStatus Status =>
        IsArchived ? FormStatus.Archived
        : PublishedVersion > 0 ? FormStatus.Published
        : FormStatus.Draft;

    public bool IsOpenForResponses => !IsArchived && PublishedVersion > 0;

    public void Apply(FormDefinitionCreated @event)
    {
        Id = @event.FormId;
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Code = @event.Code;
        Name = @event.Name;
        Description = @event.Description;
        Kind = @event.Kind;
        Cardinality = @event.Cardinality;
        SubjectKind = @event.SubjectKind;
        CreatedBy = @event.CreatedBy;
        CreatedAt = @event.CreatedAt;
        _pages = [];
    }

    public void Apply(FormDetailsUpdated @event)
    {
        Name = @event.Name;
        Description = @event.Description;
        Kind = @event.Kind;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(FormDraftSaved @event)
    {
        _pages = [.. @event.Pages];
        HasUnpublishedChanges = true;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(FormPublished @event)
    {
        // The published copy is what the draft becomes: system fields freshly resolved at that moment.
        _pages = [.. @event.Pages];
        PublishedVersion = @event.Version;
        HasUnpublishedChanges = false;
        Touch(@event.PublishedBy, @event.PublishedAt);
    }

    public void Apply(FormArchived @event)
    {
        IsArchived = true;
        Touch(@event.ArchivedBy, @event.ArchivedAt);
    }

    private void Touch(UserSnapshot by, DateTimeOffset at)
    {
        ModifiedBy = by;
        ModifiedAt = at;
    }
}
