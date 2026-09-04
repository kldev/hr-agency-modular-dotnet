using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.JobApplication.Create;
using HrAgencySystem.Recruitment.Domain.Candidate.ValueObjects;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Port;


public interface ICandidateResolver
{
    Task<CandidateInfo> FindOrCreate(CreateCandidate candidate, JobPostInfo? info, CancellationToken ct);
}

public sealed record CandidateInfo(Guid CandidateId, Email Email, CandidatePhoneNumber PhoneNumber);