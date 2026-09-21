using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Marten.Events.Projections;

namespace HrAgencySystem.Workers.Projections;

/// <summary>
/// Builds <see cref="WorkerProjection"/> out of two kinds of stream: the person's own, and every
/// assignment of theirs.
/// <para>
/// A multi stream projection rather than a snapshot, following <c>PipelineProjection</c>, because
/// the question the list has to answer - which country is this person working in - is a fact about
/// their posting and Marten cannot join two documents at query time. Carrying it here is what lets
/// the register be paged and filtered as one list.
/// </para>
/// </summary>
public sealed class WorkerProjector : MultiStreamProjection<WorkerProjection, Guid>
{
    public WorkerProjector()
    {
        Identity<WorkerRegistered>(@event => @event.WorkerId);
        Identity<WorkerUpdated>(@event => @event.WorkerId);
        Identity<WorkerStatusChanged>(@event => @event.WorkerId);
        Identity<WorkerDocumentAttached>(@event => @event.WorkerId);
        Identity<WorkerDocumentMetadataChanged>(@event => @event.WorkerId);
        Identity<WorkerDocumentRemoved>(@event => @event.WorkerId);
        Identity<WorkAuthorisationRecorded>(@event => @event.WorkerId);
        Identity<WorkAuthorisationRemoved>(@event => @event.WorkerId);

        // The assignment events land on the person they are about, which is the whole point.
        Identity<AssignmentPlanned>(@event => @event.WorkerId);
        Identity<AssignmentUpdated>(@event => @event.WorkerId);
        Identity<AssignmentStatusChanged>(@event => @event.WorkerId);
        Identity<AssignmentPositionRenamed>(@event => @event.WorkerId);
    }

    public WorkerProjection Create(WorkerRegistered @event)
    {
        var worker = new WorkerProjection
        {
            Id = @event.WorkerId,
            OrganizationId = @event.OrganizationId,
            Status = WorkerStatus.Recruitment,
            Department = WorkerStatusChangePolicy.OwnerOf(WorkerStatus.Recruitment),
            SourceCandidateId = @event.SourceCandidateId,
            CreatedById = @event.CreatedBy.Id,
            CreatedBy = @event.CreatedBy,
            CreatedAt = @event.CreatedAt,
        };

        Describe(
            worker,
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

        return worker;
    }

    public void Apply(WorkerProjection worker, WorkerUpdated @event)
    {
        Describe(
            worker,
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

        Touch(worker, @event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerProjection worker, WorkerStatusChanged @event)
    {
        worker.Status = @event.Status;
        worker.Department = WorkerStatusChangePolicy.OwnerOf(@event.Status);

        Touch(worker, @event.ChangedBy, @event.ChangedAt);
    }

    public void Apply(WorkerProjection worker, WorkerDocumentAttached @event)
    {
        WithDocuments(worker, [.. worker.Documents, @event.Document]);
        Touch(worker, @event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerProjection worker, WorkerDocumentMetadataChanged @event)
    {
        WithDocuments(
            worker,
            [
                .. worker.Documents.Select(d =>
                    d.DocumentId == @event.Document.DocumentId ? @event.Document : d
                ),
            ]
        );
        Touch(worker, @event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerProjection worker, WorkerDocumentRemoved @event)
    {
        WithDocuments(worker, [.. worker.Documents.Where(d => d.DocumentId != @event.DocumentId)]);
        Touch(worker, @event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerProjection worker, WorkAuthorisationRecorded @event)
    {
        var others = worker.Authorisations.Where(a =>
            a.AuthorisationId != @event.Authorisation.AuthorisationId
        );

        worker.Authorisations = [.. others, @event.Authorisation];
        worker.RecalculateAuthorisationFacts();

        Touch(worker, @event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerProjection worker, WorkAuthorisationRemoved @event)
    {
        worker.Authorisations =
        [
            .. worker.Authorisations.Where(a => a.AuthorisationId != @event.AuthorisationId),
        ];
        worker.RecalculateAuthorisationFacts();

        Touch(worker, @event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkerProjection worker, AssignmentPlanned @event)
    {
        var summary = new WorkerAssignmentSummary(
            @event.AssignmentId,
            @event.Project.ProjectId,
            @event.Project.ProjectName,
            @event.Project.ClientCompanyName,
            @event.Project.DeliveringEntityName,
            @event.Project.WorkCountry,
            @event.EngagementType,
            @event.Position.PositionId,
            @event.Position.Name,
            @event.StartsOn,
            @event.EndsOn,
            AssignmentStatus.Planned
        );

        worker.Assignments =
        [
            .. worker.Assignments.Where(a => a.AssignmentId != @event.AssignmentId),
            summary,
        ];
        worker.RecalculateAssignmentFacts();
    }

    public void Apply(WorkerProjection worker, AssignmentUpdated @event) =>
        Replace(
            worker,
            @event.AssignmentId,
            summary =>
                summary with
                {
                    PositionId = @event.Position.PositionId,
                    PositionName = @event.Position.Name,
                    StartsOn = @event.StartsOn,
                    EndsOn = @event.EndsOn,
                }
        );

    public void Apply(WorkerProjection worker, AssignmentPositionRenamed @event) =>
        Replace(
            worker,
            @event.AssignmentId,
            summary => summary with { PositionName = @event.Name }
        );

    public void Apply(WorkerProjection worker, AssignmentStatusChanged @event) =>
        Replace(
            worker,
            @event.AssignmentId,
            summary => summary with { Status = @event.Status, EndsOn = @event.EndsOn }
        );

    private static void Replace(
        WorkerProjection worker,
        Guid assignmentId,
        Func<WorkerAssignmentSummary, WorkerAssignmentSummary> change
    )
    {
        worker.Assignments =
        [
            .. worker.Assignments.Select(a => a.AssignmentId == assignmentId ? change(a) : a),
        ];
        worker.RecalculateAssignmentFacts();
    }

    private static void WithDocuments(
        WorkerProjection worker,
        IReadOnlyList<WorkerDocument> documents
    )
    {
        worker.Documents = documents;
        worker.DocumentCount = documents.Count;
    }

    private static void Describe(
        WorkerProjection worker,
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
        worker.FirstName = firstName;
        worker.LastName = lastName;
        worker.FullName = $"{firstName} {lastName}".Trim();
        worker.DateOfBirth = dateOfBirth;
        worker.Citizenship = citizenship;
        worker.RequiresLegalisation = LegalisationPolicy.RequiresLegalisation(citizenship);
        worker.IdentityDocumentKind = identityDocument.Kind;
        worker.IdentityDocumentNumber = identityDocument.Number;
        worker.IdentityDocumentIssuingCountry = identityDocument.IssuingCountry;
        worker.IdentityDocumentValidUntil = identityDocument.ValidUntil;
        worker.Email = email;
        worker.PhoneNumber = phoneNumber;
        worker.PhoneDigits = OnlyDigits(phoneNumber);
        worker.Address = address;
        worker.Note = note;
    }

    private static string OnlyDigits(string? value) => new([.. (value ?? "").Where(char.IsDigit)]);

    private static void Touch(WorkerProjection worker, UserSnapshot by, DateTimeOffset at)
    {
        worker.ModifiedById = by.Id;
        worker.ModifiedBy = by;
        worker.ModifiedAt = at;
    }
}
