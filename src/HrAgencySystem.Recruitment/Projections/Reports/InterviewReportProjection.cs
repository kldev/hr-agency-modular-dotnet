using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Reports.ReadModel;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Recruitment.Projections.Reports;

/// <summary>Projects the interview stream into the reporting table.</summary>
public sealed class InterviewReportProjection
    : EfCoreSingleStreamProjection<InterviewReportRow, Guid, InterviewsReportDbContext>
{
    public override InterviewReportRow? ApplyEvent(
        InterviewReportRow? snapshot,
        Guid identity,
        IEvent @event,
        InterviewsReportDbContext dbContext,
        IQuerySession session
    )
    {
        switch (@event.Data)
        {
            case InterviewCreated created:
                return new InterviewReportRow
                {
                    Id = created.InterviewId,
                    OrganizationId = created.OrganizationId,
                    JobApplicationId = created.JobApplicationId,
                    Status = nameof(InterviewStatus.Planned),
                    CreatedAt = created.OccurredAt,
                    UpdatedAt = created.OccurredAt,
                };

            case InterviewStatusChanged changed when snapshot is not null:
                snapshot.Status = changed.NewStatus.ToString();

                if (changed.NewStatus == InterviewStatus.Completed)
                {
                    snapshot.CompletedAt ??= changed.OccurredAt;
                }

                return Touch(snapshot, changed.OccurredAt);

            case not null when snapshot is not null:
                return Touch(snapshot, @event.Timestamp);

            default:
                return snapshot;
        }
    }

    private static InterviewReportRow Touch(InterviewReportRow row, DateTimeOffset at)
    {
        if (at > row.UpdatedAt)
        {
            row.UpdatedAt = at;
        }

        return row;
    }
}
