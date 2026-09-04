using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidate;

public sealed record CandidateTagged(Guid CandidateId, Tag Tag, UserSnapshot Author, DateTimeOffset CreatedAt);
