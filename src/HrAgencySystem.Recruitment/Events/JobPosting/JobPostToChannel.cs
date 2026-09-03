using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostToChannel(
    Guid JobPostId,
    PostingChannelType ChannelType,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;
