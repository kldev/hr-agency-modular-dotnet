using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Create;

public sealed record CreateJobApplication(
    Guid JobPostingId,
    string Email,
    string PhoneNumber,
    CandidateSource Source,
    DateTimeOffset CreatedAt);
