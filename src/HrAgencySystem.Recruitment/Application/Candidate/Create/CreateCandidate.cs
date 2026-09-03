using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Application.Candidate.Create;

public sealed record CreateCandidate(string Email, CandidateSource Source, string PhoneNumber, DateTimeOffset CreatedAt);