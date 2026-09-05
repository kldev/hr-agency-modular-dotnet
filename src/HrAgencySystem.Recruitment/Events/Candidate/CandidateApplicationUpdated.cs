namespace HrAgencySystem.Recruitment.Events.Candidate;

public sealed record CandidateApplicationUpdated(Guid CandidateId, Guid JobPostId, Guid CompanyId)
{
    public Guid EventId { get; init; } = Guid.NewGuid();
};
