using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Events.JobApplication;

public sealed record JobApplicationCreated(
    Guid JobApplicationId,
    Guid OrganizationId,
    Guid JobPostingId,
    Guid CandidateId,
    CandidateSource Source,
    DateTimeOffset CreatedAt,
    string Email);