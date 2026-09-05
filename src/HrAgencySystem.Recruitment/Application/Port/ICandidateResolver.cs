using HrAgencySystem.Recruitment.Application.Candidate.Create;

namespace HrAgencySystem.Recruitment.Application.Port;


public interface ICandidateResolver
{
    Task<CandidateInfo> FindOrCreate(CreateCandidate candidate, JobPostInfo info, CancellationToken ct);
}

public sealed record CandidateInfo(Guid CandidateId, string Email, string PhoneNumber, string FirstName, string LastName);

