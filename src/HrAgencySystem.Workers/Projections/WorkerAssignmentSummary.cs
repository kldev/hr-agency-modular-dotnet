using HrAgencySystem.Compliance;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Projections;

/// <summary>
/// One line of a person's employment history. The history is not a thing anybody maintains - it is
/// what the list of somebody's assignments already is, which is the whole reason the two are
/// separate aggregates.
/// </summary>
public sealed record WorkerAssignmentSummary(
    Guid AssignmentId,
    Guid ProjectId,
    string ProjectName,
    string ClientCompanyName,
    string DeliveringEntityName,
    string WorkCountry,
    EngagementType EngagementType,
    string Position,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    AssignmentStatus Status
)
{
    public bool Occupies => AssignmentStatusChangePolicy.OccupiesWorker(Status);
}
