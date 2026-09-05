using HrAgencySystem.Recruitment.Domain.Candidates;

namespace HrAgencySystem.Recruitment.Application.Candidate.UpdateApplication;

public sealed record UpdateApplication(Guid CandidateId, Guid JobPostId, Guid CompanyId, CandidateSource Source)
{
    public Guid EventId { get; init; } = Guid.NewGuid();

}
