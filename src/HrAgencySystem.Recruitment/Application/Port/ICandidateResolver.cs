using HrAgencySystem.Recruitment.Application.Candidate.Create;
namespace HrAgencySystem.Recruitment.Application.Port;

public interface ICandidateResolver
{
    Domain.Candidate.Candidate FindOrCreate(CreateCandidate candidate);
}