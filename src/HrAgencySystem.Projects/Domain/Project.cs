using HrAgencySystem.Projects.Domain.Compliance;
using HrAgencySystem.Projects.Domain.ValueObjects;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// The delivery of a sold service to a client: what happens after the sale, as opposed to the sale
/// itself. Two different life cycles, two different sets of rules, two different sets of people -
/// which is why this is its own aggregate and not another opportunity stage.
/// <para>
/// No decisions are taken here. <c>Apply</c> replays what happened and cannot see intent; every rule
/// lives in a handler, which is the only place that holds both the state before and the command.
/// </para>
/// </summary>
public sealed class Project : IOrganizationDomain
{
    private Project() { }

    public static Project Empty()
    {
        return new Project();
    }

    public ProjectId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }

    public CompanySnapshot Company { get; private set; } = null!;

    /// <summary>Our side of the engagement - see <see cref="DeliveringEntitySnapshot"/>.</summary>
    public DeliveringEntitySnapshot DeliveringEntity { get; private set; } = null!;
    public ProjectName Name { get; private set; } = null!;
    public LongText Description { get; private set; } = null!;
    public ProjectStatus Status { get; private set; }

    public EngagementType EngagementType { get; private set; }
    public Assignment Assignment { get; private set; } = null!;

    /// <summary>
    /// The team is a pointer and stays a pointer. A team exists precisely so that assignments
    /// survive rotation, so freezing its members here would defeat the point.
    /// </summary>
    public Guid? TeamId { get; private set; }

    public string? TeamName { get; private set; }

    // Marten rehydrates an aggregate without running field initialisers, so these are nullable
    // behind non-null accessors. A stream that has never seen a contact event has no contacts, and
    // no reader should have to know that means null.
    private List<ProjectContact>? _contacts;
    private List<ProjectEmailRecipient>? _emailRecipients;
    private List<ProjectDocument>? _documents;
    private List<ComplianceItem>? _compliance;

    public IReadOnlyList<ProjectContact> Contacts => _contacts ?? [];
    public IReadOnlyList<ProjectEmailRecipient> EmailRecipients => _emailRecipients ?? [];
    public IReadOnlyList<ProjectDocument> Documents => _documents ?? [];
    public IReadOnlyList<ComplianceItem> Compliance => _compliance ?? [];

    public ProjectContract? Contract { get; private set; }

    public UserSnapshot CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public UserSnapshot? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    public ProjectContact? ContactInRole(ContactRole role) =>
        Contacts.FirstOrDefault(c => c.Role == role);

    public ProjectDocument? DocumentById(Guid documentId) =>
        Documents.FirstOrDefault(d => d.DocumentId == documentId);

    public void Apply(ProjectCreated @event)
    {
        Id = ProjectId.From(@event.ProjectId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Company = @event.Company;
        DeliveringEntity = @event.DeliveringEntity;
        Name = ProjectName.Create(@event.Name);
        Description = LongText.Create(@event.Description, false);
        Status = ProjectStatus.Draft;
        EngagementType = @event.EngagementType;
        Assignment = @event.Assignment;
        TeamId = @event.TeamId;
        TeamName = @event.TeamName;
        CreatedBy = @event.CreatedBy;
        CreatedAt = @event.CreatedAt;
    }

    public void Apply(ProjectLegalEntityChanged @event)
    {
        DeliveringEntity = @event.DeliveringEntity;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectUpdated @event)
    {
        Name = ProjectName.Create(@event.Name);
        Description = LongText.Create(@event.Description, false);
        Assignment = @event.Assignment;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectStatusChanged @event)
    {
        Status = @event.Status;
        Touch(@event.ChangedBy, @event.ChangedAt);
    }

    public void Apply(ProjectTeamAssigned @event)
    {
        TeamId = @event.TeamId;
        TeamName = @event.TeamName;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectContactAssigned @event)
    {
        // One person per role: assigning is replacing, never appending.
        var contacts = Contacts.Where(c => c.Role != @event.Role).ToList();
        contacts.Add(new ProjectContact(@event.Role, @event.Person, @event.CompanyContactId));

        _contacts = contacts;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectContactRemoved @event)
    {
        _contacts = [.. Contacts.Where(c => c.Role != @event.Role)];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectEmailRecipientsChanged @event)
    {
        var others = EmailRecipients.Where(r => r.Purpose != @event.Purpose);
        var replacement = @event.Emails.Select(e => new ProjectEmailRecipient(@event.Purpose, e));

        _emailRecipients = [.. others, .. replacement];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectContractRecorded @event)
    {
        Contract = @event.Contract;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectContractStatusChanged @event)
    {
        if (Contract is not null)
            Contract = Contract with { Status = @event.Status, SignedOn = @event.SignedOn };

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectDocumentAttached @event)
    {
        _documents = [.. Documents, @event.Document];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectDocumentMetadataChanged @event)
    {
        _documents =
        [
            .. Documents.Select(d =>
                d.DocumentId == @event.Document.DocumentId ? @event.Document : d
            ),
        ];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ProjectDocumentRemoved @event)
    {
        _documents = [.. Documents.Where(d => d.DocumentId != @event.DocumentId)];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(ComplianceItemRecorded @event)
    {
        var others = Compliance.Where(c => c.Requirement != @event.Item.Requirement);

        _compliance = [.. others, @event.Item];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    private void Touch(UserSnapshot by, DateTimeOffset at)
    {
        ModifiedBy = by;
        ModifiedAt = at;
    }
}
