using HrAgencySystem.Recruitment.Domain.Candidate;

namespace HrAgencySystem.Recruitment.Events.Candidate;

public sealed record CandidateCreated(Guid Id, string Email, string PhoneNumber, CandidateSource Source);