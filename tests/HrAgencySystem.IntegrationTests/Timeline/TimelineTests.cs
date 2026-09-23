using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Application.Timeline.Queries;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Projections.Timeline;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Timeline;

[Collection(IntegrationCollection.Name)]
public sealed class TimelineTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private const string StatusNote = "SECRET-status-note";
    private const string ApplicationNote = "SECRET-application-note";

    private static readonly TimeSpan ProjectionTimeout = TimeSpan.FromSeconds(15);

    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _otherOrganizationId = Guid.NewGuid();

    private TimelineTestClient TimelineClient => new(Client);

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTimeline();
        await Cleaner.CleanCandidates();
        await Cleaner.CleanJobApplications();
    }

    [Fact]
    public async Task CandidateTimeline_AggregatesEveryApplication_NewestFirst()
    {
        var history = await BuildHistoryAsync();

        await Eventually.AssertAsync(
            async () =>
            {
                var page = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );
                var items = page.Slice.Content;

                Assert.Equal(
                    [
                        TimelineEntryType.NoteAdded,
                        TimelineEntryType.InterviewCanceled,
                        // Same instant as the interview, appended after it.
                        TimelineEntryType.ApplicationStatusChanged,
                        TimelineEntryType.InterviewScheduled,
                        TimelineEntryType.NoteAdded,
                        TimelineEntryType.ApplicationStatusChanged,
                        TimelineEntryType.ApplicationCreated,
                        TimelineEntryType.ApplicationCreated,
                        TimelineEntryType.CandidateCreated,
                    ],
                    items.Select(x => x.Type)
                );
                Assert.Null(page.Slice.NextCursor);

                for (var i = 1; i < items.Count; i++)
                    Assert.True(items[i - 1].OccurredAt >= items[i].OccurredAt);

                Assert.Equal(
                    2,
                    items.Select(x => x.JobApplicationId).OfType<Guid>().Distinct().Count()
                );
                Assert.Null(items[^1].JobApplicationId);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task CandidateTimeline_ShowsStatusTransitions_IncludingTheOneSchedulingMade()
    {
        var history = await BuildHistoryAsync();

        await Eventually.AssertAsync(
            async () =>
            {
                var page = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );

                var transitions = page
                    .Slice.Content.Where(x => x.Type == TimelineEntryType.ApplicationStatusChanged)
                    .Select(x => (x.From, x.To))
                    .ToList();

                // The second one has no JobApplicationStatusChanged behind it: scheduling the
                // interview moved the application.
                Assert.Equal(
                    [
                        (
                            nameof(JobApplicationStatus.Screening),
                            nameof(JobApplicationStatus.Interview)
                        ),
                        (
                            nameof(JobApplicationStatus.Applied),
                            nameof(JobApplicationStatus.Screening)
                        ),
                    ],
                    transitions
                );
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task CandidateTimeline_ShowsInterviewsWithTheirApplicationAndPost()
    {
        var history = await BuildHistoryAsync();

        await Eventually.AssertAsync(
            async () =>
            {
                var page = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );

                var scheduled = Assert.Single(
                    page.Slice.Content,
                    x => x.Type == TimelineEntryType.InterviewScheduled
                );
                Assert.Equal(history.InterviewId, scheduled.InterviewId);
                Assert.Equal(history.FirstApplicationId, scheduled.JobApplicationId);
                Assert.Equal(history.FirstJobPostId, scheduled.JobPostId);
                Assert.Equal(InterviewType.Technical, scheduled.InterviewType);
                Assert.NotNull(scheduled.ScheduledAt);

                var canceled = Assert.Single(
                    page.Slice.Content,
                    x => x.Type == TimelineEntryType.InterviewCanceled
                );
                Assert.Equal(history.FirstApplicationId, canceled.JobApplicationId);
                Assert.Equal(nameof(InterviewStatus.Canceled), canceled.To);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task CandidateTimeline_ShowsThatNotesWereAdded_NeverWhatTheySay()
    {
        var history = await BuildHistoryAsync();

        await Eventually.AssertAsync(
            async () =>
            {
                var page = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );

                var notes = page
                    .Slice.Content.Where(x => x.Type == TimelineEntryType.NoteAdded)
                    .ToList();
                Assert.Equal(2, notes.Count);
                Assert.All(notes, x => Assert.Null(x.From));
                Assert.All(notes, x => Assert.Null(x.To));

                Assert.DoesNotContain("SECRET", page.Raw);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task CandidateTimeline_OnTheSameInstant_OrdersByEventSequence()
    {
        var history = await BuildHistoryAsync();

        await Eventually.AssertAsync(
            async () =>
            {
                var first = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );
                var second = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );

                Assert.Equal(9, first.Slice.Content.Count);
                Assert.Equal(
                    first.Slice.Content.Select(x => x.Id),
                    second.Slice.Content.Select(x => x.Id)
                );

                // A status change with a note is one command and one instant; the note was
                // appended after the status event, so it comes first.
                var items = first.Slice.Content.ToList();
                var note = items.FindLastIndex(x => x.Type == TimelineEntryType.NoteAdded);
                Assert.Equal(TimelineEntryType.ApplicationStatusChanged, items[note + 1].Type);
                Assert.Equal(items[note].OccurredAt, items[note + 1].OccurredAt);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task ApplicationTimeline_ShowsOnlyThatApplication()
    {
        var history = await BuildHistoryAsync();

        await Eventually.AssertAsync(
            async () =>
            {
                var first = await TimelineClient.GetApplicationTimelineAsync(
                    _organizationId,
                    history.FirstApplicationId
                );
                Assert.Equal(
                    [
                        TimelineEntryType.InterviewCanceled,
                        // Same instant as the interview, appended after it.
                        TimelineEntryType.ApplicationStatusChanged,
                        TimelineEntryType.InterviewScheduled,
                        TimelineEntryType.NoteAdded,
                        TimelineEntryType.ApplicationStatusChanged,
                        TimelineEntryType.ApplicationCreated,
                    ],
                    first.Slice.Content.Select(x => x.Type)
                );
                Assert.All(
                    first.Slice.Content,
                    x => Assert.Equal(history.FirstApplicationId, x.JobApplicationId)
                );

                var second = await TimelineClient.GetApplicationTimelineAsync(
                    _organizationId,
                    history.SecondApplicationId
                );
                Assert.Equal(
                    [TimelineEntryType.NoteAdded, TimelineEntryType.ApplicationCreated],
                    second.Slice.Content.Select(x => x.Type)
                );
                Assert.DoesNotContain("SECRET", second.Raw);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task Timelines_AreEmptyForAnotherOrganization()
    {
        var history = await BuildHistoryAsync();

        // Wait until the history exists, so an empty answer below means isolation, not lag.
        await Eventually.AssertAsync(
            async () =>
            {
                var own = await TimelineClient.GetCandidateTimelineAsync(
                    _organizationId,
                    history.CandidateId
                );
                Assert.Equal(9, own.Slice.Content.Count);
            },
            ProjectionTimeout,
            output: OutputHelper
        );

        var candidate = await TimelineClient.GetCandidateTimelineAsync(
            _otherOrganizationId,
            history.CandidateId
        );
        var application = await TimelineClient.GetApplicationTimelineAsync(
            _otherOrganizationId,
            history.FirstApplicationId
        );

        Assert.Empty(candidate.Slice.Content);
        Assert.Null(candidate.Slice.NextCursor);
        Assert.Empty(application.Slice.Content);
    }

    [Fact]
    public async Task CandidateTimeline_PagesByCursor_WithoutGapsOrDuplicates()
    {
        var history = await BuildHistoryAsync();

        TimelineSlice full = null!;
        await Eventually.AssertAsync(
            async () =>
            {
                full = (
                    await TimelineClient.GetCandidateTimelineAsync(
                        _organizationId,
                        history.CandidateId
                    )
                ).Slice;
                Assert.Equal(9, full.Content.Count);
            },
            ProjectionTimeout,
            output: OutputHelper
        );

        var firstPage = (
            await TimelineClient.GetCandidateTimelineAsync(
                _organizationId,
                history.CandidateId,
                pageSize: 4
            )
        ).Slice;
        Assert.Equal(4, firstPage.Content.Count);
        Assert.NotNull(firstPage.NextCursor);

        var secondPage = (
            await TimelineClient.GetCandidateTimelineAsync(
                _organizationId,
                history.CandidateId,
                firstPage.NextCursor,
                pageSize: 4
            )
        ).Slice;
        Assert.Equal(4, secondPage.Content.Count);
        Assert.NotNull(secondPage.NextCursor);

        var lastPage = (
            await TimelineClient.GetCandidateTimelineAsync(
                _organizationId,
                history.CandidateId,
                secondPage.NextCursor,
                pageSize: 4
            )
        ).Slice;
        Assert.Single(lastPage.Content);
        Assert.Null(lastPage.NextCursor);

        var paged = firstPage
            .Content.Concat(secondPage.Content)
            .Concat(lastPage.Content)
            .Select(x => x.Id)
            .ToList();
        Assert.Equal(paged.Count, paged.Distinct().Count());
        Assert.Equal(full.Content.Select(x => x.Id), paged);
    }

    [Fact]
    public async Task CandidateTimeline_RefusesACursorItDidNotIssue()
    {
        var status = await TimelineClient.GetCandidateTimelineStatusAsync(
            _organizationId,
            Guid.NewGuid(),
            "not-a-cursor"
        );

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    /// <summary>
    /// One candidate, two applications. The first moves Applied → Screening with a note, gets an
    /// interview (which moves it to Interview) and the interview is cancelled; the second gets
    /// a note. Nine entries in all, counting the candidate's own creation.
    /// </summary>
    private async Task<History> BuildHistoryAsync()
    {
        var email = $"timeline-{Guid.NewGuid():N}@test.io";

        var firstPost = await TimelineClient.PublishedJobPostAsync(_organizationId);
        var secondPost = await TimelineClient.PublishedJobPostAsync(_organizationId);

        var first = await TimelineClient.ApplyAsync(_organizationId, firstPost, email);
        var second = await TimelineClient.ApplyAsync(_organizationId, secondPost, email);
        Assert.Equal(first.CandidateInfo.CandidateId, second.CandidateInfo.CandidateId);

        await TimelineClient.ChangeStatusAsync(
            _organizationId,
            first.JobApplicationId,
            JobApplicationUpdateStatus.Screening,
            StatusNote
        );
        var interview = await TimelineClient.ScheduleInterviewAsync(
            _organizationId,
            first.JobApplicationId,
            DateTime.UtcNow.Date.AddDays(14).AddHours(10)
        );
        await TimelineClient.ChangeInterviewStatusAsync(
            _organizationId,
            interview.InterviewId,
            InterviewStatus.Canceled
        );
        await TimelineClient.AddNoteAsync(
            _organizationId,
            second.JobApplicationId,
            ApplicationNote
        );

        return new History(
            first.CandidateInfo.CandidateId,
            first.JobApplicationId,
            firstPost,
            second.JobApplicationId,
            interview.InterviewId
        );
    }

    private sealed record History(
        Guid CandidateId,
        Guid FirstApplicationId,
        Guid FirstJobPostId,
        Guid SecondApplicationId,
        Guid InterviewId
    );
}
