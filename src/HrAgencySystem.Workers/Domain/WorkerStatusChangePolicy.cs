namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// How a person moves through the pipeline, in the shape <c>ProjectStatusChangePolicy</c> already
/// uses: a pure static holding the graph, with everything that needs outside state left to the
/// handler.
/// <para>
/// The graph is not fixed, and that is the point. Legalisation sits between the contract and
/// onboarding for somebody who needs it and does not exist at all for somebody who does not, so the
/// policy is asked the question with the answer to <see cref="LegalisationPolicy"/> in hand rather
/// than branching on a country itself.
/// </para>
/// </summary>
public static class WorkerStatusChangePolicy
{
    public static bool Allow(
        WorkerStatus currentStatus,
        WorkerStatus newStatus,
        bool requiresLegalisation
    )
    {
        if (currentStatus == newStatus)
            return false;

        // Somebody can drop out of any stage, and somebody who never arrived leaves the same way as
        // somebody who worked for two years. The difference between the two is the stage they were
        // in when it happened, which the history keeps.
        if (newStatus is WorkerStatus.Terminated)
            return currentStatus is not WorkerStatus.Terminated;

        return (currentStatus, newStatus) switch
        {
            (WorkerStatus.Recruitment, WorkerStatus.ContractPreparation) => true,

            (WorkerStatus.ContractPreparation, WorkerStatus.Legalisation) => requiresLegalisation,
            (WorkerStatus.ContractPreparation, WorkerStatus.Onboarding) => !requiresLegalisation,

            (WorkerStatus.Legalisation, WorkerStatus.Onboarding) => true,

            (WorkerStatus.Onboarding, WorkerStatus.Employed) => true,

            // Moving somebody to another project. They keep everything they already hold; what the
            // move needs is arranged here, and how much of it there is depends on where they are
            // going - which is why all three ways out of it are open.
            (WorkerStatus.Employed, WorkerStatus.ProjectChange) => true,
            (WorkerStatus.ProjectChange, WorkerStatus.Employed) => true,
            (WorkerStatus.ProjectChange, WorkerStatus.Onboarding) => true,
            (WorkerStatus.ProjectChange, WorkerStatus.Legalisation) => requiresLegalisation,

            // Back to the desk that owns the previous stage. Papers expire, a permit lapses, an
            // address falls through - the person does not restart, they go back a step.
            (WorkerStatus.Employed, WorkerStatus.Legalisation) => requiresLegalisation,
            (WorkerStatus.Employed, WorkerStatus.Onboarding) => true,

            // Rehiring somebody we parted with. A new contract has to be prepared, so that is where
            // they come back in - never into recruitment, which already happened.
            (WorkerStatus.Terminated, WorkerStatus.ContractPreparation) => true,

            _ => false,
        };
    }

    /// <summary>
    /// Whose desk a person in this stage is on. A dictionary rather than a switch at every call
    /// site, so adding a stage is a row and adding a department is a value.
    /// </summary>
    private static readonly Dictionary<WorkerStatus, ResponsibleDepartment> Owners = new()
    {
        [WorkerStatus.Recruitment] = ResponsibleDepartment.Recruitment,
        [WorkerStatus.ContractPreparation] = ResponsibleDepartment.HumanResources,
        [WorkerStatus.Legalisation] = ResponsibleDepartment.Legalisation,
        [WorkerStatus.Onboarding] = ResponsibleDepartment.Operations,
        [WorkerStatus.Employed] = ResponsibleDepartment.Operations,
        [WorkerStatus.ProjectChange] = ResponsibleDepartment.Operations,
        [WorkerStatus.Terminated] = ResponsibleDepartment.None,
    };

    public static ResponsibleDepartment OwnerOf(WorkerStatus status) =>
        Owners.GetValueOrDefault(status, ResponsibleDepartment.None);

    /// <summary>
    /// Whether somebody in this stage may be put on a project. Planning ahead is allowed while the
    /// paperwork runs - that is how the work actually gets scheduled - but nobody starts work before
    /// the pipeline has finished with them. Somebody mid transfer counts as ready: arranging the
    /// move is what that stage is, and the move ends with the new assignment going live.
    /// </summary>
    public static bool MayStartWork(WorkerStatus status) =>
        status is WorkerStatus.Employed or WorkerStatus.ProjectChange;

    public static bool MayBePlanned(WorkerStatus status) => status is not WorkerStatus.Terminated;
}
