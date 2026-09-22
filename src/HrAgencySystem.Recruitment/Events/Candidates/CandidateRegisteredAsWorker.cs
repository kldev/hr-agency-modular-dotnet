namespace HrAgencySystem.Recruitment.Events.Candidates;

/// <summary>
/// This person now has a file in the workers' register. A fact about the human being rather than
/// about one application - somebody with three applications is still one worker.
/// </summary>
public sealed record CandidateRegisteredAsWorker(
    Guid CandidateId,
    Guid WorkerId,
    DateTimeOffset OccurredAt
);
