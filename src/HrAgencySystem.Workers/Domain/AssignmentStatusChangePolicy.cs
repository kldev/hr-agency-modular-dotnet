namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// Which status changes an assignment allows. Same shape as <c>ProjectStatusChangePolicy</c>: the
/// graph lives here, and the conditions that need state this class cannot see live in the handler.
/// </summary>
public static class AssignmentStatusChangePolicy
{
    public static bool Allow(AssignmentStatus currentStatus, AssignmentStatus newStatus)
    {
        if (currentStatus == newStatus)
            return false;

        if (IsFinal(currentStatus))
            return false;

        return (currentStatus, newStatus) switch
        {
            (AssignmentStatus.Planned, AssignmentStatus.Active) => true,

            // Said they would go and did not. Recorded rather than deleted, because a crew that has
            // to be replaced twice is something the next plan needs to know about.
            (AssignmentStatus.Planned, AssignmentStatus.DidNotStart) => true,

            (AssignmentStatus.Active, AssignmentStatus.Completed) => true,
            (AssignmentStatus.Active, AssignmentStatus.Interrupted) => true,

            _ => false,
        };
    }

    /// <summary>
    /// A finished stay does not restart. Somebody going back to the same client next year is a new
    /// assignment - new period, new A1, new everything the first one needed.
    /// </summary>
    public static bool IsFinal(AssignmentStatus status) =>
        status
            is AssignmentStatus.Completed
                or AssignmentStatus.Interrupted
                or AssignmentStatus.DidNotStart;

    /// <summary>
    /// Whether an assignment in this state still takes up the person's calendar. Nobody works two
    /// positions at once, so this is what a second assignment over the same days is checked against.
    /// One that never started frees the days back up.
    /// </summary>
    public static bool OccupiesWorker(AssignmentStatus status) => !IsFinal(status);
}
