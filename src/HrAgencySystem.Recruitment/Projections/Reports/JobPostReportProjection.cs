using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Reports.ReadModel;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Recruitment.Projections.Reports;

/// <summary>
/// Projects the job post stream into the reporting table. Only whether the post is out and when it
/// first went out matter to a report; the content stays in the feed and the UI read models.
/// </summary>
public sealed class JobPostReportProjection
    : EfCoreSingleStreamProjection<JobPostReportRow, Guid, JobPostsReportDbContext>
{
    public override JobPostReportRow? ApplyEvent(
        JobPostReportRow? snapshot,
        Guid identity,
        IEvent @event,
        JobPostsReportDbContext dbContext,
        IQuerySession session
    )
    {
        switch (@event.Data)
        {
            case JobPostCreated created:
                return new JobPostReportRow
                {
                    Id = created.JobPostId,
                    OrganizationId = created.OrganizationId,
                    CompanyId = created.CompanyId,
                    IsPublished = false,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.CreatedAt,
                };

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
                    status.OccurredAt
                );

            case not null when snapshot is not null:
                return Touch(snapshot, @event.Timestamp);

            default:
                return snapshot;
        }
    }

    // Map*/Touch rather than Create/Apply - see JobPostFeedProjection for why the names matter.
    private static JobPostReportRow Publish(
        JobPostReportRow row,
        bool isPublished,
        DateTimeOffset at
    )
    {
        row.IsPublished = isPublished;

        if (isPublished)
        {
            row.FirstPublishedAt ??= at;
        }

        return Touch(row, at);
    }

    private static JobPostReportRow Touch(JobPostReportRow row, DateTimeOffset at)
    {
        if (at > row.UpdatedAt)
        {
            row.UpdatedAt = at;
        }

        return row;
    }
}
