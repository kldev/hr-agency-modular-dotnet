using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// A person. Everything here is true of the human being regardless of who they are working for
/// today: their name, when they were born, which passport they hold, which permits they have and
/// how far through our pipeline they are.
/// <para>
/// What is deliberately absent: a project, a client, a delivering company, a position and a period.
/// All five are true of a posting rather than of a person, and they live on <see cref="Assignment"/>.
/// Somebody moving from a Polish project to a German one is this same record with a second
/// assignment - which is what makes their history a history rather than an overwritten field.
/// </para>
/// <para>
/// No decisions are taken here. <c>Apply</c> replays what happened and cannot see intent; every rule
/// lives in a handler, which is the only place that holds both the state before and the command.
/// </para>
/// </summary>
public sealed class Worker : IOrganizationDomain
{
    private Worker() { }

    public static Worker Empty()
    {
        return new Worker();
    }

    public WorkerId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }

    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public CountryCode Citizenship { get; private set; } = null!;
    public IdentityDocument IdentityDocument { get; private set; } = null!;

    public Email? Email { get; private set; }
    public PersonPhone PhoneNumber { get; private set; } = null!;
    public PostalAddress? Address { get; private set; }
    public LongText Note { get; private set; } = null!;

    public WorkerStatus Status { get; private set; }

    /// <summary>Where the file came from, when it came from a candidate. Never a foreign key.</summary>
    public Guid? SourceCandidateId { get; private set; }

    // Marten rehydrates an aggregate without running field initialisers, so these are nullable
    // behind non-null accessors. A file that has never seen a document event has no documents, and
    // no reader should have to know that means null.
    private List<WorkerDocument>? _documents;
    private List<WorkAuthorisation>? _authorisations;

    public IReadOnlyList<WorkerDocument> Documents => _documents ?? [];
    public IReadOnlyList<WorkAuthorisation> Authorisations => _authorisations ?? [];

    public UserSnapshot CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public UserSnapshot? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    public string FullName => $"{FirstName.Value} {LastName.Value}".Trim();

    /// <summary>Worked out from the citizenship rather than stored - see <see cref="LegalisationPolicy"/>.</summary>
    public bool RequiresLegalisation => LegalisationPolicy.RequiresLegalisation(Citizenship);

    public WorkerDocument? DocumentById(Guid documentId) =>
        Documents.FirstOrDefault(d => d.DocumentId == documentId);

    public WorkAuthorisation? AuthorisationById(Guid authorisationId) =>
        Authorisations.FirstOrDefault(a => a.AuthorisationId == authorisationId);

    public void Apply(WorkerRegistered @event)
    {
        Id = WorkerId.From(@event.WorkerId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Status = WorkerStatus.Recruitment;
        SourceCandidateId = @event.SourceCandidateId;
        CreatedBy = @event.CreatedBy;
        CreatedAt = @event.CreatedAt;

        Describe(
            @event.FirstName,
            @event.LastName,
            @event.DateOfBirth,
            @event.Citizenship,
            @event.IdentityDocument,
            @event.Email,
            @event.PhoneNumber,
            @event.Address,
            @event.Note
        );
    }

    public void Apply(WorkerUpdated @event)
    {
        Describe(
            @event.FirstName,
            @event.LastName,
            @event.DateOfBirth,
            @event.Citizenship,
            @event.IdentityDocument,
            @event.Email,
            @event.PhoneNumber,
            @event.Address,
            @event.Note
        );

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerStatusChanged @event)
    {
        Status = @event.Status;
        Touch(@event.ChangedBy, @event.ChangedAt);
    }

    public void Apply(WorkerDocumentAttached @event)
    {
        _documents = [.. Documents, @event.Document];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerDocumentMetadataChanged @event)
    {
        _documents =
        [
            .. Documents.Select(d =>
                d.DocumentId == @event.Document.DocumentId ? @event.Document : d
            ),
        ];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerDocumentRemoved @event)
    {
        _documents = [.. Documents.Where(d => d.DocumentId != @event.DocumentId)];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkAuthorisationRecorded @event)
    {
        // Recording is an upsert: a renewed permit under the same id replaces its own earlier terms,
        // while a genuinely new one arrives with a new id and stands beside the old.
        var others = Authorisations.Where(a =>
            a.AuthorisationId != @event.Authorisation.AuthorisationId
        );

        _authorisations = [.. others, @event.Authorisation];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkAuthorisationRemoved @event)
    {
        _authorisations =
        [
            .. Authorisations.Where(a => a.AuthorisationId != @event.AuthorisationId),
        ];
        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    private void Describe(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string citizenship,
        IdentityDocument identityDocument,
        string? email,
        string phoneNumber,
        PostalAddress? address,
        string note
    )
    {
        FirstName = FirstName.Create(firstName);
        LastName = LastName.Create(lastName);
        DateOfBirth = dateOfBirth;
        Citizenship = CountryCode.Create(citizenship);
        IdentityDocument = identityDocument;
        Email = string.IsNullOrWhiteSpace(email) ? null : Email.Create(email);
        PhoneNumber = PersonPhone.Create(phoneNumber);
        Address = address;
        Note = LongText.Create(note, false);
    }

    private void Touch(UserSnapshot by, DateTimeOffset at)
    {
        ModifiedBy = by;
        ModifiedAt = at;
    }
}
