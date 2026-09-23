using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Projections.Timeline;

namespace HrAgencySystem.Recruitment.Application.Timeline.Queries;

/// <summary>
/// A candidate's or one application's history, newest first, paged by keyset: a new entry
/// written while somebody scrolls lands before their cursor instead of shifting every page.
/// </summary>
public interface ITimelineQueryRepository
{
    Task<TimelineSlice> GetCandidateTimeline(
        Guid organizationId,
        Guid candidateId,
        TimelineCursor? after,
        int pageSize,
        CancellationToken ct
    );

    /// <summary>Only this application and its interviews - not the person's other applications.</summary>
    Task<TimelineSlice> GetApplicationTimeline(
        Guid organizationId,
        Guid jobApplicationId,
        TimelineCursor? after,
        int pageSize,
        CancellationToken ct
    );
}

public sealed record TimelineItem(
    Guid Id,
    TimelineEntryType Type,
    DateTimeOffset OccurredAt,
    string? AuthorName,
    Guid? JobApplicationId,
    Guid? JobPostId,
    string? JobPostTitle,
    Guid? InterviewId,
    string? From,
    string? To,
    InterviewType? InterviewType,
    DateTimeOffset? ScheduledAt
);

/// <summary><see cref="NextCursor"/> is null on the last page.</summary>
public sealed record TimelineSlice(IReadOnlyList<TimelineItem> Content, string? NextCursor);
