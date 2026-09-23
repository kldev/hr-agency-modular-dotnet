using HrAgencySystem.Recruitment.Application.Timeline.Queries;
using HrAgencySystem.Recruitment.Projections.Timeline;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public sealed class TimelineQueryRepository(IQuerySession session) : ITimelineQueryRepository
{
    public Task<TimelineSlice> GetCandidateTimeline(
        Guid organizationId,
        Guid candidateId,
        TimelineCursor? after,
        int pageSize,
        CancellationToken ct
    ) =>
        Page(
            session
                .Query<TimelineEntry>()
                .Where(x => x.OrgId == organizationId && x.CandidateId == candidateId),
            after,
            pageSize,
            ct
        );

    public Task<TimelineSlice> GetApplicationTimeline(
        Guid organizationId,
        Guid jobApplicationId,
        TimelineCursor? after,
        int pageSize,
        CancellationToken ct
    ) =>
        Page(
            session
                .Query<TimelineEntry>()
                .Where(x => x.OrgId == organizationId && x.JobApplicationId == jobApplicationId),
            after,
            pageSize,
            ct
        );

    private static async Task<TimelineSlice> Page(
        IQueryable<TimelineEntry> query,
        TimelineCursor? after,
        int pageSize,
        CancellationToken ct
    )
    {
        var size = Math.Clamp(pageSize, 1, 500);

        if (after is not null)
        {
            var at = after.OccurredAt;
            var sequence = after.Sequence;
            query = query.Where(x =>
                x.OccurredAt < at || (x.OccurredAt == at && x.Sequence < sequence)
            );
        }

        // One row more than asked for says whether there is another page, without a count.
        var entries = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Sequence)
            .Take(size + 1)
            .ToListAsync(ct);

        var page = entries.Take(size).ToList();
        var next =
            entries.Count > size
                ? new TimelineCursor(page[^1].OccurredAt, page[^1].Sequence).Encode()
                : null;

        return new TimelineSlice([.. page.Select(ToItem)], next);
    }

    private static TimelineItem ToItem(TimelineEntry entry) =>
        new(
            entry.Id,
            entry.Type,
            entry.OccurredAt,
            entry.AuthorName,
            entry.JobApplicationId,
            entry.JobPostId,
            entry.JobPostTitle,
            entry.InterviewId,
            entry.From,
            entry.To,
            entry.InterviewType,
            entry.ScheduledAt
        );
}
