namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// The life of one person's stay on one project. Five states, because five things actually happen:
/// it is arranged, it is running, it ran its course, it was cut short - and, separately, it never
/// began at all.
/// </summary>
public enum AssignmentStatus
{
    Planned,
    Active,

    /// <summary>Ran to its end. The period on the record is what actually happened.</summary>
    Completed,

    /// <summary>Started and was cut short.</summary>
    Interrupted,

    /// <summary>
    /// Arranged and then nobody turned up. Kept apart from <see cref="Interrupted"/> on purpose:
    /// somebody who said they would go and did not is a different problem from somebody who went and
    /// came back early, and the two are told apart by whoever plans the next crew.
    /// </summary>
    DidNotStart,
}
