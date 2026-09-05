using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Create;

public sealed record CreateJobApplication(
    Guid JobPostId,
    Guid EventId,
    string Email,
    string Phone,
    CandidateSource Source,
    string? FirstName = null,
    string? LastName = null);
