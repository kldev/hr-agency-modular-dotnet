using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// One person on one project, in one position, over one period, under one way of working. Everything
/// here is true of that posting and of nothing else.
/// <para>
/// Its own stream rather than a collection on <see cref="Worker"/>, for the reason <c>JobPost</c> is
/// its own stream and not a list on <c>JobDescription</c>: it has a life of its own, it carries its
/// own documents and its own obligations, and it is read from the project's side as often as from
/// the person's.
/// </para>
/// <para>
/// Nothing here can be repointed. Moving somebody from one project to another ends this assignment
/// and opens another one; editing the project out of an existing record would erase the only proof
/// that the first posting ever happened, which is precisely what an A1 register cannot afford.
/// </para>
/// </summary>
public sealed class Assignment : IOrganizationDomain
{
    private Assignment() { }

    public static Assignment Empty()
    {
        return new Assignment();
    }

    public AssignmentId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }

    public WorkerId WorkerId { get; private set; }

    /// <summary>The person's name as it stood when they were posted - see <see cref="AssignmentPlanned"/>.</summary>
    public string WorkerFullName { get; private set; } = null!;

    public ProjectPlacementSnapshot Project { get; private set; } = null!;

    /// <summary>
    /// How this person works here, which is not necessarily how everybody on the project does. One
    /// delivery can post some people and employ others under local law, and the obligations follow
    /// each person's own arrangement rather than the contract's headline.
    /// </summary>
    public EngagementType EngagementType { get; private set; }

    public AssignmentPosition Position { get; private set; } = null!;

    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }

    public AssignmentStatus Status { get; private set; }

    private List<AssignmentDocument>? _documents;
    private List<ComplianceItem>? _compliance;

    public IReadOnlyList<AssignmentDocument> Documents => _documents ?? [];
    public IReadOnlyList<ComplianceItem> Compliance => _compliance ?? [];

    public UserSnapshot CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public UserSnapshot? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    public AssignmentDocument? DocumentById(Guid documentId) =>
        Documents.FirstOrDefault(d => d.DocumentId == documentId);

    public void Apply(AssignmentPlanned @event)
    {
        Id = AssignmentId.From(@event.AssignmentId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        WorkerId = WorkerId.From(@event.WorkerId);
        WorkerFullName = @event.WorkerFullName;
        Project = @event.Project;
        EngagementType = @event.EngagementType;
        Position = @event.Position;
        StartsOn = @event.StartsOn;
        EndsOn = @event.EndsOn;
        Status = AssignmentStatus.Planned;
        CreatedBy = @event.CreatedBy;
        CreatedAt = @event.CreatedAt;
    }

    /// <summary>
    /// The role kept its id and changed its name. Nothing the assignment decides depends on it,
    /// which is exactly why the name is allowed to follow the project while the id never does.
    /// </summary>
    public void Apply(AssignmentPositionRenamed @event)
    {
        Position = Position with { Name = @event.Name, ContractName = @event.ContractName };
    }

    public void Apply(AssignmentUpdated @event)
    {
        Position = @event.Position;
        StartsOn = @event.StartsOn;
        EndsOn = @event.EndsOn;
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(AssignmentStatusChanged @event)
    {
        Status = @event.Status;
        EndsOn = @event.EndsOn;
        Touch(@event.ChangedBy, @event.ChangedAt);
    }

    public void Apply(AssignmentDocumentAttached @event)
    {
        _documents = [.. Documents, @event.Document];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(AssignmentDocumentMetadataChanged @event)
    {
        _documents =
        [
            .. Documents.Select(d =>
                d.DocumentId == @event.Document.DocumentId ? @event.Document : d
            ),
        ];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(AssignmentDocumentRemoved @event)
    {
        _documents = [.. Documents.Where(d => d.DocumentId != @event.DocumentId)];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(AssignmentComplianceItemRecorded @event)
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
