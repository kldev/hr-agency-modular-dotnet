namespace HrAgencySystem.Recruitment.Events.Applications;

/// <summary>
/// The workers' file was opened from this application. Says which conversation it came out of;
/// the status is left alone, because opening a file and hiring somebody are separate decisions.
/// <para>
/// Not an <see cref="IJobApplicationEvent"/>: it arrives from another module, which does not say
/// who pressed the button, and inventing an author would be worse than having none.
/// </para>
/// </summary>
public sealed record JobApplicationRegisteredAsWorker(
    Guid JobApplicationId,
    Guid WorkerId,
    DateTimeOffset OccurredAt
);
