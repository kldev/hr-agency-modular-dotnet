namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// Which status changes the domain allows, in the shape <c>JobPostStatusChangePolicy</c> already
/// uses. The extra conditions on going live - a signed contract, somebody responsible, a complete
/// client profile - are not here: they need state this class cannot see, so they sit in the handler.
/// </summary>
public static class ProjectStatusChangePolicy
{
    public static bool Allow(ProjectStatus currentStatus, ProjectStatus newStatus)
    {
        if (currentStatus == newStatus)
            return false;

        if (IsFinal(currentStatus))
            return false;

        return (currentStatus, newStatus) switch
        {
            (ProjectStatus.Draft, ProjectStatus.Active) => true,
            (ProjectStatus.Draft, ProjectStatus.Cancelled) => true,

            (ProjectStatus.Active, ProjectStatus.Suspended) => true,
            (ProjectStatus.Active, ProjectStatus.Completed) => true,
            (ProjectStatus.Active, ProjectStatus.Cancelled) => true,

            (ProjectStatus.Suspended, ProjectStatus.Active) => true,
            (ProjectStatus.Suspended, ProjectStatus.Completed) => true,
            (ProjectStatus.Suspended, ProjectStatus.Cancelled) => true,

            _ => false,
        };
    }

    /// <summary>
    /// A project that ended does not restart. Reopening one would hide that the cooperation stopped
    /// and started again, which is exactly what somebody reading the history needs to see.
    /// </summary>
    public static bool IsFinal(ProjectStatus status) =>
        status is ProjectStatus.Completed or ProjectStatus.Cancelled;
}
