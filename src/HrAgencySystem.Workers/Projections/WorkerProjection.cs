using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Application.Suggestion;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Projections;

/// <summary>
/// One read model for the list and for the file, the way <c>ProjectProjection</c> already works
/// here.
/// <para>
/// A mutable class rather than a record, because this one is fed by <see cref="WorkerProjector"/>
/// from two streams at once - the person's and every assignment of theirs - and that is Marten's
/// shape for a multi stream projection. It is also why <see cref="CurrentWorkCountry"/> can exist at
/// all: the country somebody works in is a fact about their posting, and a list that has to be split
/// between the people looking after Poland and the people looking after everywhere else needs it on
/// this row rather than behind a join nobody can page through.
/// </para>
/// </summary>
public sealed class WorkerProjection
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string Citizenship { get; set; } = "";

    /// <summary>Worked out from the citizenship, so it cannot drift from it - see <see cref="LegalisationPolicy"/>.</summary>
    public bool RequiresLegalisation { get; set; }

    public IdentityDocumentKind IdentityDocumentKind { get; set; }
    public string IdentityDocumentNumber { get; set; } = "";
    public string IdentityDocumentIssuingCountry { get; set; } = "";
    public DateOnly? IdentityDocumentValidUntil { get; set; }

    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = "";

    /// <summary>
    /// The phone with everything but the digits taken out, so that the same number written two
    /// ways still identifies the same person. Only ever used to spot a second file for somebody
    /// already on the books.
    /// </summary>
    public string PhoneDigits { get; set; } = "";
    public PostalAddress? Address { get; set; }
    public string Note { get; set; } = "";

    public WorkerStatus Status { get; set; }

    /// <summary>Whose desk this file is on. Flattened because it is what a work queue filters by.</summary>
    public ResponsibleDepartment Department { get; set; }

    public Guid? SourceCandidateId { get; set; }
    public Guid? SourceApplicationId { get; set; }

    public IReadOnlyList<WorkerDocument> Documents { get; set; } = [];
    public int DocumentCount { get; set; }

    public IReadOnlyList<WorkAuthorisation> Authorisations { get; set; } = [];

    /// <summary>The first permit to run out. The one question legalisation asks of a list.</summary>
    public DateOnly? NextAuthorisationExpiryOn { get; set; }

    public IReadOnlyList<WorkerAssignmentSummary> Assignments { get; set; } = [];

    /// <summary>
    /// Where this person is working, or is about to. The assignment that is running takes
    /// precedence; failing that, the next one planned; failing that, the last one there was.
    /// </summary>
    public string? CurrentWorkCountry { get; set; }
    public Guid? CurrentAssignmentId { get; set; }
    public string? CurrentProjectName { get; set; }
    public int AssignmentCount { get; set; }
    public int OpenAssignmentCount { get; set; }

    public Guid CreatedById { get; set; }
    public UserSnapshot CreatedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? ModifiedById { get; set; }
    public UserSnapshot? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public WorkerSuggestion ToSuggestion() => new(Id, FullName, Status);

    /// <summary>
    /// Recomputed whenever the assignments change. "Where is this person" is answered from the
    /// history rather than stored beside it, so the two cannot disagree.
    /// <para>
    /// Deliberately without a notion of today. A projection is written when something happens, not
    /// every morning, so a rule that depended on the date would be right when it ran and wrong the
    /// following week. Status is what moves, so status is what this reads.
    /// </para>
    /// </summary>
    public void RecalculateAssignmentFacts()
    {
        AssignmentCount = Assignments.Count;
        OpenAssignmentCount = Assignments.Count(a => a.Occupies);

        var current =
            Assignments.FirstOrDefault(a => a.Status == AssignmentStatus.Active)
            ?? Assignments.Where(a => a.Occupies).OrderBy(a => a.StartsOn).FirstOrDefault()
            ?? Assignments.OrderByDescending(a => a.EndsOn ?? a.StartsOn).FirstOrDefault();

        CurrentAssignmentId = current?.AssignmentId;
        CurrentProjectName = current?.ProjectName;
        CurrentWorkCountry = current?.WorkCountry;
    }

    public void RecalculateAuthorisationFacts()
    {
        NextAuthorisationExpiryOn =
            Authorisations.Select(a => a.ValidUntil).DefaultIfEmpty().Min() is var earliest
            && earliest == default
                ? null
                : earliest;
    }
}
