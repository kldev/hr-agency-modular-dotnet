using HrAgencySystem.Recruitment.Domain.Candidates;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Create;

public sealed record ApplyToJobApplication(
    Guid JobPostId,
    string Email,
    string Phone,
    CandidateSource Source,
    string? FirstName = null,
    string? LastName = null)
{
    public string ToFullName()
        => $"{FirstName} {LastName}".Trim();
}
