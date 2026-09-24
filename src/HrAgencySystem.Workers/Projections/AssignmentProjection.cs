using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.Workers.Projections;

/// <summary>
/// The posting record: one row per person per project, with its own documents and its own
/// obligations.
/// <para>
/// Read from the project's side - who is on this delivery, whose A1 is missing - where the person's
/// own file answers the other direction. The two overlap by a few columns on purpose; the
/// alternative is a join Marten cannot do at query time.
/// </para>
/// </summary>
public sealed record AssignmentProjection(
    Guid Id,
    Guid OrganizationId,
    Guid WorkerId,
    string WorkerFullName,
    Guid ProjectId,
    string ProjectName,
    Guid ClientCompanyId,
    string ClientCompanyName,
    Guid DeliveringEntityId,
    string DeliveringEntityName,
    string WorkCountry,
    EngagementType EngagementType,
    Guid PositionId,
    // The role's internal name, flattened out of the snapshot the way the project's is: a list
    // filters and sorts on it, and a name nested in an object costs more in Marten than a column.
    string PositionName,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    AssignmentStatus Status,
    IReadOnlyList<AssignmentDocument> Documents,
    IReadOnlyList<ComplianceItem> Compliance,
    int DocumentCount,
    int ComplianceRequiredCount,
    int ComplianceOutstandingCount,
    DateOnly? NextComplianceExpiryOn,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    public static AssignmentProjection Create(AssignmentPlanned @event) =>
        new AssignmentProjection(
            @event.AssignmentId,
            @event.OrganizationId,
            @event.WorkerId,
            @event.WorkerFullName,
            @event.Project.ProjectId,
            @event.Project.ProjectName,
            @event.Project.ClientCompanyId,
            @event.Project.ClientCompanyName,
            @event.Project.DeliveringEntityId,
            @event.Project.DeliveringEntityName,
            @event.Project.WorkCountry,
            @event.EngagementType,
            @event.Position.PositionId,
            @event.Position.Name,
            @event.StartsOn,
            @event.EndsOn,
            AssignmentStatus.Planned,
            [],
            [],
            0,
            0,
            0,
            null,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            null,
            null,
            null
        ).WithComplianceCounts();

    public AssignmentProjection Apply(AssignmentUpdated @event) =>
        (
            this with
            {
                PositionId = @event.Position.PositionId,
                PositionName = @event.Position.Name,
                StartsOn = @event.StartsOn,
                EndsOn = @event.EndsOn,
            }
        ).Touched(@event.ModifiedBy, @event.ModifiedAt);

    /// <summary>
    /// The role kept its id and changed its name. No audit stamp: nobody here touched this record,
    /// the project did, and claiming otherwise would put a stranger's name on somebody's posting.
    /// </summary>
    public AssignmentProjection Apply(AssignmentPositionRenamed @event) =>
        this with
        {
            PositionName = @event.Name,
        };

    public AssignmentProjection Apply(AssignmentStatusChanged @event) =>
        (this with { Status = @event.Status, EndsOn = @event.EndsOn }).Touched(
            @event.ChangedBy,
            @event.ChangedAt
        );

    public AssignmentProjection Apply(AssignmentDocumentAttached @event) =>
        WithDocuments([.. Documents, @event.Document])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public AssignmentProjection Apply(AssignmentDocumentMetadataChanged @event) =>
        WithDocuments([
                .. Documents.Select(d =>
                    d.DocumentId == @event.Document.DocumentId ? @event.Document : d
                ),
            ])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public AssignmentProjection Apply(AssignmentDocumentRemoved @event) =>
        WithDocuments([.. Documents.Where(d => d.DocumentId != @event.DocumentId)])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public AssignmentProjection Apply(AssignmentComplianceItemRecorded @event)
    {
        var others = Compliance.Where(c => c.Requirement != @event.Item.Requirement);

        return (this with { Compliance = [.. others, @event.Item] })
            .WithComplianceCounts()
            .Touched(@event.ModifiedBy, @event.ModifiedAt);
    }

    private AssignmentProjection WithDocuments(IReadOnlyList<AssignmentDocument> documents) =>
        this with
        {
            Documents = documents,
            DocumentCount = documents.Count,
        };

    /// <summary>
    /// What this posting still owes, counted from the catalogue at the assignment's own level. This
    /// is the register CLAUDE.md said was missing: outstanding here means one named person is short
    /// of one named certificate, which is a thing somebody can go and fix.
    /// </summary>
    private AssignmentProjection WithComplianceCounts()
    {
        var required = ComplianceCatalogue.For(
            WorkCountry,
            EngagementType,
            ComplianceScope.Assignment
        );
        var settled = Compliance.Where(c => c.IsSettled).Select(c => c.Requirement).ToHashSet();

        return this with
        {
            ComplianceRequiredCount = required.Count,
            ComplianceOutstandingCount = required.Count(r => !settled.Contains(r)),
            NextComplianceExpiryOn =
                Compliance
                    .Where(c => c.Status is ComplianceStatus.Confirmed && c.ValidTo is not null)
                    .Select(c => c.ValidTo!.Value)
                    .DefaultIfEmpty()
                    .Min()
                    is var earliest
                && earliest == default
                    ? null
                    : earliest,
        };
    }

    private AssignmentProjection Touched(UserSnapshot by, DateTimeOffset at) =>
        this with
        {
            ModifiedById = by.Id,
            ModifiedBy = by,
            ModifiedAt = at,
        };
}
