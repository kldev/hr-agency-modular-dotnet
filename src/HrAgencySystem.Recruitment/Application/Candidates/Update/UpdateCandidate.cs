using HrAgencySystem.Recruitment.Application.Candidates.Create;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Candidates.Update;

public sealed record UpdateCandidate(
    Guid CandidateId,
    Guid OrganizationId,
    string Phone,
    string FirstName,
    string LastName,
    string Note,
    Guid ModifiedBy):IUpdateCommand, ICandidateData;