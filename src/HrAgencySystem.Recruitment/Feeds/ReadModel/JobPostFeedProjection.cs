using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Recruitment.Feeds.ReadModel;

/// <summary>
/// Projects the job post stream into <see cref="FeedsDbContext"/>'s relational table.
/// <para>
/// <see cref="JobPostedToChannel"/> and <see cref="JobPostRecruiterChanged"/> are intentionally
/// not handled - the feed does not expose either.
/// </para>
/// </summary>
public sealed class JobPostFeedProjection
    : EfCoreSingleStreamProjection<JobPostFeedRow, Guid, FeedsDbContext>
{
    public override JobPostFeedRow? ApplyEvent(
        JobPostFeedRow? snapshot,
        Guid identity,
        IEvent @event,
        FeedsDbContext dbContext,
        IQuerySession session)
    {
        switch (@event.Data)
        {
            case JobPostCreated created:
                return JobPostFeedRow.From(created);

            case JobPostUpdated updated when snapshot is not null:
                snapshot.Update(updated);
                return snapshot;

            case JobPostPublished published when snapshot is not null:
                return Publish(snapshot, true, published.OccurredAt);

            case JobPostClosed closed when snapshot is not null:
                return Publish(snapshot, false, closed.OccurredAt);

            case JobPostArchived archived when snapshot is not null:
                return Publish(snapshot, false, archived.OccurredAt);

            case JobPostStatusChanged status when snapshot is not null:
                return Publish(
                    snapshot,
                    status.NewStatus == JobPostStatus.Published,
                    status.OccurredAt);

            default:
                return snapshot;
        }
    }

    private static JobPostFeedRow Publish(
        JobPostFeedRow row,
        bool isPublished,
        DateTimeOffset occurredAt)
    {
        row.IsPublished = isPublished;
        row.UpdatedAt = occurredAt;

        return row;
    }
}
