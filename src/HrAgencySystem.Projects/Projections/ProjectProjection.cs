using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.Compliance;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Projects.Projections;

/// <summary>
/// One read model for both the list and the details page, the way <c>CompanyProjection</c> and
/// <c>TeamProjection</c> already work here. A second projection would mean a second daemon and a
/// second chance for the two to drift apart, and the details page is a single GET either way.
/// <para>
/// A handful of fields are flattened out of the collections - the responsible contact, the two
/// counts, the nearest expiry. They are what a list filters and sorts on, and filtering on a field
/// nested inside an array costs more in Marten than carrying one more column.
/// </para>
/// </summary>
public sealed record ProjectProjection(
    Guid Id,
    Guid OrganizationId,
    Guid CompanyId,
    string CompanyName,
    string CompanyTaxId,
    DeliveringEntitySnapshot DeliveringEntity,
    string Name,
    string Description,
    ProjectStatus Status,
    EngagementType EngagementType,
    string WorkCountry,
    PostalAddress WorkplaceAddress,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    Guid? TeamId,
    string? TeamName,
    IReadOnlyList<ProjectContact> Contacts,
    ContactPerson? ResponsibleContact,
    IReadOnlyList<ProjectEmailRecipient> EmailRecipients,
    ProjectContract? Contract,
    IReadOnlyList<ProjectDocument> Documents,
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
    public static ProjectProjection Create(ProjectCreated @event)
    {
        return new ProjectProjection(
            @event.ProjectId,
            @event.OrganizationId,
            @event.Company.Id,
            @event.Company.Name,
            @event.Company.TaxId,
            @event.DeliveringEntity,
            @event.Name,
            @event.Description,
            ProjectStatus.Draft,
            @event.EngagementType,
            @event.Assignment.WorkCountry,
            @event.Assignment.WorkplaceAddress,
            @event.Assignment.StartsOn,
            @event.Assignment.EndsOn,
            @event.TeamId,
            @event.TeamName,
            [],
            null,
            [],
            null,
            [],
            [],
            0,
            ComplianceCatalogue.For(@event.Assignment.WorkCountry, @event.EngagementType).Count,
            ComplianceCatalogue.For(@event.Assignment.WorkCountry, @event.EngagementType).Count,
            null,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            null,
            null,
            null
        ).WithComplianceCounts();
    }

    public ProjectProjection Apply(ProjectLegalEntityChanged @event)
    {
        return this with
        {
            DeliveringEntity = @event.DeliveringEntity,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public ProjectProjection Apply(ProjectUpdated @event) =>
        (
            this with
            {
                Name = @event.Name,
                Description = @event.Description,
                WorkCountry = @event.Assignment.WorkCountry,
                WorkplaceAddress = @event.Assignment.WorkplaceAddress,
                StartsOn = @event.Assignment.StartsOn,
                EndsOn = @event.Assignment.EndsOn,
            }
        )
            .WithComplianceCounts()
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ProjectStatusChanged @event) =>
        (this with { Status = @event.Status }).Touched(@event.ChangedBy, @event.ChangedAt);

    public ProjectProjection Apply(ProjectTeamAssigned @event) =>
        (this with { TeamId = @event.TeamId, TeamName = @event.TeamName }).Touched(
            @event.ModifiedBy,
            @event.ModifiedAt
        );

    public ProjectProjection Apply(ProjectContactAssigned @event)
    {
        var contacts = Contacts.Where(c => c.Role != @event.Role).ToList();
        contacts.Add(new ProjectContact(@event.Role, @event.Person, @event.CompanyContactId));

        return WithContacts(contacts).Touched(@event.ModifiedBy, @event.ModifiedAt);
    }

    public ProjectProjection Apply(ProjectContactRemoved @event) =>
        WithContacts([.. Contacts.Where(c => c.Role != @event.Role)])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ProjectEmailRecipientsChanged @event)
    {
        var others = EmailRecipients.Where(r => r.Purpose != @event.Purpose);
        var replacement = @event.Emails.Select(e => new ProjectEmailRecipient(@event.Purpose, e));

        return (this with { EmailRecipients = [.. others, .. replacement] }).Touched(
            @event.ModifiedBy,
            @event.ModifiedAt
        );
    }

    public ProjectProjection Apply(ProjectContractRecorded @event) =>
        (this with { Contract = @event.Contract }).Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ProjectContractStatusChanged @event) =>
        (
            this with
            {
                Contract = Contract is null
                    ? null
                    : Contract with
                    {
                        Status = @event.Status,
                        SignedOn = @event.SignedOn,
                    },
            }
        ).Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ProjectDocumentAttached @event) =>
        WithDocuments([.. Documents, @event.Document])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ProjectDocumentMetadataChanged @event) =>
        WithDocuments([
                .. Documents.Select(d =>
                    d.DocumentId == @event.Document.DocumentId ? @event.Document : d
                ),
            ])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ProjectDocumentRemoved @event) =>
        WithDocuments([.. Documents.Where(d => d.DocumentId != @event.DocumentId)])
            .Touched(@event.ModifiedBy, @event.ModifiedAt);

    public ProjectProjection Apply(ComplianceItemRecorded @event)
    {
        var others = Compliance.Where(c => c.Requirement != @event.Item.Requirement);

        return (this with { Compliance = [.. others, @event.Item] })
            .WithComplianceCounts()
            .Touched(@event.ModifiedBy, @event.ModifiedAt);
    }

    public ProjectSuggestion ToSuggestion() => new(Id, Name, CompanyName, Status);

    private ProjectProjection WithContacts(IReadOnlyList<ProjectContact> contacts) =>
        this with
        {
            Contacts = contacts,
            ResponsibleContact = contacts
                .FirstOrDefault(c => c.Role == ContactRole.Responsible)
                ?.Person,
        };

    private ProjectProjection WithDocuments(IReadOnlyList<ProjectDocument> documents) =>
        this with
        {
            Documents = documents,
            DocumentCount = documents.Count,
        };

    /// <summary>
    /// Recomputed whenever the catalogue's inputs or the recorded items change. "How much is still
    /// open" and "what expires next" are the two things a list has to show without opening a
    /// project, which is the whole reason this module exists.
    /// </summary>
    private ProjectProjection WithComplianceCounts()
    {
        var required = ComplianceCatalogue.For(WorkCountry, EngagementType);
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

    private ProjectProjection Touched(UserSnapshot by, DateTimeOffset at) =>
        this with
        {
            ModifiedById = by.Id,
            ModifiedBy = by,
            ModifiedAt = at,
        };
}
