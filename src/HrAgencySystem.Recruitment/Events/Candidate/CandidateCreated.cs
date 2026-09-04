using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidate;

public sealed record CandidateCreated(Guid Id, 
    Guid OrganizationId, 
    string Email, 
    string PhoneNumber, 
    CandidateSource Source, 
    DateTimeOffset CreatedAt,
    UserSnapshot? CreatedBy,
    Guid? CompanyId);