using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostedToChannel(
    Guid JobPostId,
    PostingChannelType ChannelType,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;
