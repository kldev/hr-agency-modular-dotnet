namespace HrAgencySystem.Recruitment.Domain.Posting;

public sealed record ChannelPost(PostingChannelType ChannelType, DateTimeOffset PublishedAt);