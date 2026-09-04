using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidate;

public sealed record CandidateTagRemoved(Guid CandidateId, Tag Tag, UserSnapshot Author);