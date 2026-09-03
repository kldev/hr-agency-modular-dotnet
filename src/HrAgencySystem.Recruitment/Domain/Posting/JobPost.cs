namespace HrAgencySystem.Recruitment.Domain.Posting;

public sealed record JobPost(JobPostingChannel Channel, DateTimeOffset AddedAt);