using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Create;

public sealed record CreateJobApplication(
    Guid JobPostId,
    string Email,
    string PhoneNumber,
    CandidateSource Source,
    string? FirstName = null,
    string? LastName = null);
