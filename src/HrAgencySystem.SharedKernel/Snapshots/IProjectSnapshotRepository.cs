using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.SharedKernel.Snapshots;

/// <summary>
/// A delivery in progress, as seen by a module that does not own projects. Read by the workers
/// module, which has to put a named person on one and freeze what that engagement was at the time.
/// </summary>
public interface IProjectSnapshotRepository
{
    public const string NotFoundMessage = "Required project data not found.";

    /// <summary>
    /// Resolves a project only when it belongs to the given organization: nobody is ever assigned to
    /// another agency's delivery.
    /// </summary>
    Task<ProjectSnapshot?> GetProjectAsync(
        Guid projectId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

/// <summary>
/// What an assignment needs to know about the project it hangs off.
/// <para>
/// Deliberately without the engagement type. One project can run people under more than one mode -
/// some posted, some employed under local law - so the mode is stated on each assignment rather
/// than inherited from here, where it would look authoritative and sometimes be wrong.
/// </para>
/// <para>
/// The delivering entity is only an id and a name. An assignment records which of our companies
/// posted the person, because that is what an A1 is issued against; the full registered identity
/// stays where it belongs, in <see cref="LegalEntitySnapshot"/>.
/// </para>
/// </summary>
public sealed record ProjectSnapshot(
    Guid Id,
    string Name,
    Guid CompanyId,
    string CompanyName,
    Guid DeliveringEntityId,
    string DeliveringEntityName,
    string WorkCountry,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    bool IsOpenForAssignments
)
{
    /// <summary>
    /// Whether a period of work fits inside the delivery. A person cannot start before the project
    /// does or after it has finished, and a period with a known end cannot outlast it. An assignment
    /// left open ended is allowed on a project with an end date - it means "until further notice",
    /// and the delivery ending is what puts a stop to it.
    /// </summary>
    public bool Covers(DateOnly startsOn, DateOnly? endsOn)
    {
        if (startsOn < StartsOn)
            return false;

        if (EndsOn is null)
            return true;

        return startsOn <= EndsOn && (endsOn is null || endsOn <= EndsOn);
    }
}
