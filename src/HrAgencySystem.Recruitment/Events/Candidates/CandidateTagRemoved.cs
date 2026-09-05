using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Candidates;

// ReSharper disable once NotAccessedPositionalProperty.Global
public sealed record CandidateTagRemoved(Guid CandidateId, Tag Tag, UserSnapshot RemovedBy, DateTimeOffset ModifiedAt);