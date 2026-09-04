using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Application.Candidate.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateCandidate(Guid OrganizationId, 
    string Email, 
    CandidateSource Source, 
    string PhoneNumber, 
    Guid? CreatedBy); // can be created from application form or by user