using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

namespace HrAgencySystem.Recruitment.Projections.Timeline;

/// <summary>
/// Writes one <see cref="TimelineEntry"/> per event of the candidate, job application and
/// interview streams, so that a candidate's history - which spans many streams - is one indexed
/// query instead of a walk through every application.
/// <para>
/// Most events carry neither the organization nor the candidate. Both come from the first event
/// of the same stream (<c>CandidateCreated</c>, <c>JobApplicationCreated</c>,
/// <c>InterviewCreated</c>), read from the event store rather than from another projection, which
/// the daemon may not have caught up with yet.
/// </para>
/// </summary>
public partial class TimelineProjection : EventProjection
{
    // ---- candidate stream --------------------------------------------------------------------

    // ReSharper disable once MemberCanBePrivate.Global
    public void Project(IEvent<CandidateCreated> @event, IDocumentOperations operations)
    {
        var context = new TimelineContext(@event.Data.OrganizationId, @event.Data.CandidateId);

        operations.Store(
            Entry(
                @event,
                context,
                TimelineEntryType.CandidateCreated,
                @event.Data.CreatedAt,
                @event.Data.CreatedBy
            )
        );
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<CandidateUpdated> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.CandidateUpdated,
            @event.Data.ModifiedAt,
            @event.Data.ModifiedBy,
            ct
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<CandidateTagged> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.CandidateTagged,
            @event.Data.CreatedAt,
            @event.Data.Author,
            ct,
            entry => entry with { To = @event.Data.Tag.Name }
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<CandidateTagRemoved> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.CandidateTagRemoved,
            @event.Data.ModifiedAt,
            @event.Data.RemovedBy,
            ct,
            entry => entry with { To = @event.Data.Tag.Name }
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<CandidateRegisteredAsWorker> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.RegisteredAsWorker,
            @event.Data.OccurredAt,
            null,
            ct
        );

    // ---- job application stream --------------------------------------------------------------

    // ReSharper disable once MemberCanBePrivate.Global
    public void Project(IEvent<JobApplicationCreated> @event, IDocumentOperations operations)
    {
        operations.Store(
            Entry(
                @event,
                TimelineContext.Of(@event.Data),
                TimelineEntryType.ApplicationCreated,
                @event.Data.CreatedAt,
                @event.Data.CreatedBy
            )
        );
    }

    /*
     * Status changes are taken from the narrative events, not from JobApplicationStatusChanged:
     * scheduling an interview moves the application to Interview and reactivating it moves it
     * back to Screening, and neither appends JobApplicationStatusChanged. The status before the
     * event comes from the aggregate itself, so the transition rules stay in its Apply methods.
     */

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationScreeningStarted> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Screening, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationAssessmentStarted> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Assessment, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationInterviewScheduled> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Interview, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationOfferMade> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Offer, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationHired> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Hired, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationRejected> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Rejected, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationWithdrawn> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Withdrawn, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationReactivated> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) => RecordStatusChange(@event, operations, JobApplicationStatus.Screening, ct);

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationUpdated> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.ApplicationUpdated,
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationTagged> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.ApplicationTagged,
            @event.Data.CreatedAt,
            @event.Data.Author,
            ct,
            entry => entry with { To = @event.Data.Tag.Name }
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationTagRemoved> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.ApplicationTagRemoved,
            @event.Data.ModifiedAt,
            @event.Data.RemovedBy,
            ct,
            entry => entry with { To = @event.Data.Tag.Name }
        );

    /// <summary>That a note was added, and by whom. The note itself stays out.</summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationNoteAdded> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.NoteAdded,
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<JobApplicationRegisteredAsWorker> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.RegisteredAsWorker,
            @event.Data.OccurredAt,
            null,
            ct
        );

    // ---- interview stream --------------------------------------------------------------------

    // ReSharper disable once MemberCanBePrivate.Global
    public void Project(IEvent<InterviewCreated> @event, IDocumentOperations operations)
    {
        var entry = Entry(
            @event,
            TimelineContext.Of(@event.Data),
            TimelineEntryType.InterviewScheduled,
            @event.Data.OccurredAt,
            @event.Data.Author
        );

        operations.Store(entry with { ScheduledAt = @event.Data.ScheduleAt });
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<InterviewRescheduled> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.InterviewRescheduled,
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct,
            entry => entry with { ScheduledAt = @event.Data.ScheduleAt }
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<InterviewStatusChanged> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            InterviewEntryType(@event.Data.NewStatus),
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct,
            entry =>
                entry with
                {
                    From = @event.Data.OldStatus.ToString(),
                    To = @event.Data.NewStatus.ToString(),
                }
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<InterviewerChanged> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.InterviewerChanged,
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct,
            entry =>
                entry with
                {
                    From = @event.Data.PreviousInterview.Fullname,
                    To = @event.Data.NewInterviewer.Fullname,
                }
        );

    // ReSharper disable once MemberCanBePrivate.Global
    public Task Project(
        IEvent<InterviewFormatChanged> @event,
        IDocumentOperations operations,
        CancellationToken ct
    ) =>
        Record(
            @event,
            operations,
            TimelineEntryType.InterviewFormatChanged,
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct,
            entry =>
                entry with
                {
                    From = @event.Data.OldFormat.ToString(),
                    To = @event.Data.NewFormat.ToString(),
                }
        );

    // ---- helpers -----------------------------------------------------------------------------

    private static TimelineEntryType InterviewEntryType(InterviewStatus status) =>
        status switch
        {
            InterviewStatus.Canceled => TimelineEntryType.InterviewCanceled,
            InterviewStatus.Completed => TimelineEntryType.InterviewCompleted,
            InterviewStatus.NoShow => TimelineEntryType.InterviewNoShow,
            _ => TimelineEntryType.InterviewStatusChanged,
        };

    private static async Task RecordStatusChange<TEvent>(
        IEvent<TEvent> @event,
        IDocumentOperations operations,
        JobApplicationStatus newStatus,
        CancellationToken ct
    )
        where TEvent : IJobApplicationEvent
    {
        var before = await operations.Events.AggregateStreamAsync<JobApplication>(
            @event.StreamId,
            version: @event.Version - 1,
            token: ct
        );

        // A second interview on an application already in Interview is not a status change.
        if (before is null || before.Status == newStatus)
            return;

        await Record(
            @event,
            operations,
            TimelineEntryType.ApplicationStatusChanged,
            @event.Data.OccurredAt,
            @event.Data.Author,
            ct,
            entry => entry with { From = before.Status.ToString(), To = newStatus.ToString() }
        );
    }

    private static async Task Record(
        IEvent @event,
        IDocumentOperations operations,
        TimelineEntryType type,
        DateTimeOffset occurredAt,
        UserSnapshot? author,
        CancellationToken ct,
        Func<TimelineEntry, TimelineEntry>? details = null
    )
    {
        var context = await ContextOf(@event.StreamId, operations, ct);
        if (context is null)
            return;

        var entry = Entry(@event, context, type, occurredAt, author);
        operations.Store(details is null ? entry : details(entry));
    }

    private static async Task<TimelineContext?> ContextOf(
        Guid streamId,
        IDocumentOperations operations,
        CancellationToken ct
    )
    {
        var first = await operations.Events.FetchStreamAsync(streamId, version: 1, token: ct);

        return first.FirstOrDefault()?.Data switch
        {
            CandidateCreated candidate => new TimelineContext(
                candidate.OrganizationId,
                candidate.CandidateId
            ),
            JobApplicationCreated application => TimelineContext.Of(application),
            InterviewCreated interview => TimelineContext.Of(interview),
            _ => null,
        };
    }

    private static TimelineEntry Entry(
        IEvent @event,
        TimelineContext context,
        TimelineEntryType type,
        DateTimeOffset occurredAt,
        UserSnapshot? author
    ) =>
        new()
        {
            Id = @event.Id,
            Sequence = @event.Sequence,
            OrgId = context.OrgId,
            CandidateId = context.CandidateId,
            JobApplicationId = context.JobApplicationId,
            JobPostId = context.JobPostId,
            JobPostTitle = context.JobPostTitle,
            InterviewId = context.InterviewId,
            InterviewType = context.InterviewType,
            Type = type,
            OccurredAt = ToMicroseconds(occurredAt),
            AuthorName = string.IsNullOrWhiteSpace(author?.Fullname) ? null : author.Fullname,
        };

    /// <summary>
    /// Postgres keeps microseconds and .NET keeps ticks. A cursor carrying ticks the database
    /// rounded away would compare the boundary row against itself and repeat it on the next page.
    /// </summary>
    private static DateTimeOffset ToMicroseconds(DateTimeOffset value) =>
        value.AddTicks(-(value.Ticks % 10));

    /// <summary>Who and what an event belongs to, as stated by the first event of its stream.</summary>
    private sealed record TimelineContext(
        Guid OrgId,
        Guid CandidateId,
        Guid? JobApplicationId = null,
        Guid? JobPostId = null,
        string? JobPostTitle = null,
        Guid? InterviewId = null,
        InterviewType? InterviewType = null
    )
    {
        public static TimelineContext Of(JobApplicationCreated created) =>
            new(
                created.OrganizationId,
                created.CandidateInfo.CandidateId,
                created.JobApplicationId,
                created.JobPostId,
                created.JobPostTitle
            );

        public static TimelineContext Of(InterviewCreated created) =>
            new(
                created.OrganizationId,
                created.CandidateId,
                created.JobApplicationId,
                created.JobPostId,
                created.JobPostTitle,
                created.InterviewId,
                created.InterviewType
            );
    }
}
