namespace HrAgencySystem.Recruitment.Events.Candidates;

public sealed record CandidateApplicationUpdated(Guid CandidateId, Guid JobPostId, Guid CompanyId, DateTimeOffset OccuredAt);