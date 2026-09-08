using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidates;

public sealed record CandidateUpdated(
    Guid CandidateId,
    Guid OrganizationId,
    string Phone,
    string FirstName,
    string LastName,
    string Note, 
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt);