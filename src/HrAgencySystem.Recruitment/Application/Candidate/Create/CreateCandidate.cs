using HrAgencySystem.Recruitment.Domain.Candidates;

namespace HrAgencySystem.Recruitment.Application.Candidate.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateCandidate(
    Guid OrganizationId,
    string Email,
    CandidateSource Source,
    string Phone,
    string? FirstName = null,
    string? LastName = null,
    Guid? CreatedBy = null,
    Guid? CompanyId = null); // can be created from application form or by user