using HrAgencySystem.Recruitment.Domain.Interviews;

namespace HrAgencySystem.Recruitment.Projections.Timeline;

/// <summary>
/// One line of a candidate's history: one domain event, flattened into what the timeline shows.
/// <para>
/// Never carries the text of a note - neither the application note nor the note typed while
/// scheduling an interview or editing the candidate. What is not stored cannot be returned.
/// </para>
/// </summary>
public sealed record TimelineEntry
{
    /// <summary>The event's id, so replaying the projection overwrites instead of duplicating.</summary>
    public Guid Id { get; init; }

    /// <summary>The event's global sequence - unique, the tie-breaker for identical timestamps.</summary>
    public long Sequence { get; init; }

    public Guid OrgId { get; init; }
    public Guid CandidateId { get; init; }

    /// <summary>Null for facts about the person rather than about one application.</summary>
    public Guid? JobApplicationId { get; init; }

    public Guid? JobPostId { get; init; }

    /// <summary>Frozen at application time, like on <see cref="JobApplicationProjection"/>.</summary>
    public string? JobPostTitle { get; init; }

    public Guid? InterviewId { get; init; }
    public TimelineEntryType Type { get; init; }
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Null when the event names nobody: facts from Workers, applications from the board.</summary>
    public string? AuthorName { get; init; }

    /// <summary>The value before the change - a status, a format, an interviewer.</summary>
    public string? From { get; init; }

    /// <summary>The value after the change; the tag's name for a tag entry.</summary>
    public string? To { get; init; }

    public InterviewType? InterviewType { get; init; }
    public DateTimeOffset? ScheduledAt { get; init; }
}

public enum TimelineEntryType
{
    CandidateCreated,
    CandidateUpdated,
    CandidateTagged,
    CandidateTagRemoved,
    RegisteredAsWorker,

    ApplicationCreated,
    ApplicationStatusChanged,
    ApplicationUpdated,
    ApplicationTagged,
    ApplicationTagRemoved,
    NoteAdded,

    InterviewScheduled,
    InterviewRescheduled,
    InterviewStatusChanged,
    InterviewCanceled,
    InterviewCompleted,
    InterviewNoShow,
    InterviewerChanged,
    InterviewFormatChanged,
}
