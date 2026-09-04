using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Candidate.TagCandidate;

public sealed record TagCandidate(Guid TagId, Guid CandidateId, Guid OrganizationId, Guid CreatedBy) : ICreateCommand;
