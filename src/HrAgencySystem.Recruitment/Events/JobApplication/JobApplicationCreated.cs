using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobApplication;

public sealed record JobApplicationCreated(
    Guid JobApplicationId,
    Guid OrganizationId,
    Guid JobPostId,
    string JobPostTitle,
    CandidateSource Source,
    CompanySnapshot Company,
    CandidateInfo CandidateInfo,
    string ApplicantEmail,
    string ApplicantPhone,
    string ApplicantFirstName,
    string ApplicantLastName,
    DateTimeOffset CreatedAt)
{
    public string FullName => $"{ApplicantFirstName} {ApplicantLastName}".Trim();
}