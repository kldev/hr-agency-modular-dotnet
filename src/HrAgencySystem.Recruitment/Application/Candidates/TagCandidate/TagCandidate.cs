using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Candidates.TagCandidate;

// ReSharper disable once NotAccessedPositionalProperty.Global
public sealed record TagCandidate(Guid TagId, Guid CandidateId, Guid OrganizationId, Guid CreatedBy) : ICreateCommand;
