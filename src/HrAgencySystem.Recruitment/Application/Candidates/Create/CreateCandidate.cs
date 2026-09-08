using HrAgencySystem.Recruitment.Domain.Candidates;

namespace HrAgencySystem.Recruitment.Application.Candidates.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateCandidate(
    Guid OrganizationId,
    string Email,
    CandidateSource Source,
    string Phone,
    string FirstName = "",
    string LastName = "",
    Guid? CreatedBy = null,
    Guid? CompanyId = null,
    string Note = "") : ICandidateData;