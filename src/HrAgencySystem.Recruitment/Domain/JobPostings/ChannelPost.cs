namespace HrAgencySystem.Recruitment.Domain.JobPostings;

public sealed record ChannelPost(PostingChannelType ChannelType, DateTimeOffset PublishedAt);