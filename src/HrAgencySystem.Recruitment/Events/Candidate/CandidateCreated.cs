using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidate;

public sealed record CandidateCreated(Guid CandidateId, 
    Guid OrganizationId, 
    string Email, 
    string Phone, 
    CandidateSource Source, 
    DateTimeOffset CreatedAt,
    UserSnapshot? CreatedBy,
    Guid? CompanyId,
    string FirstName,
    string LastName
    );