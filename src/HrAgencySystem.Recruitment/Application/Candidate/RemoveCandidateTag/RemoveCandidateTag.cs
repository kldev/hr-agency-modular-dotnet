using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Candidate.RemoveCandidateTag;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record RemoveCandidateTag(Guid CandidateId, Guid TagId, Guid OrganizationId, Guid ModifiedBy) : IUpdateCommand;