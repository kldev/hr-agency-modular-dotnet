using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidates;

public sealed record CandidateTagged(Guid CandidateId, Tag Tag, UserSnapshot Author, DateTimeOffset CreatedAt);
