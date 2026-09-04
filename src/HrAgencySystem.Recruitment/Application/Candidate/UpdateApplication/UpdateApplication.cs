using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Application.Candidate.UpdateApplication;

public sealed record UpdateApplication(Guid CandidateId, Guid JobPostId, Guid CompanyId, CandidateSource Source);
