using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Reports.ReadModel;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Recruitment.Projections.Reports;

/// <summary>
/// Projects the job application stream into the reporting table that the recruitment funnel counts.
/// <para>
/// A stage is reached by its narrative event <b>and</b> by <see cref="JobApplicationStatusChanged"/>:
/// scheduling an interview moves the application without a status change event, while a change from
/// the panel raises both. Stage columns keep the first time only, so the second of the pair is a
/// no-op instead of a double count. <see cref="JobApplicationReactivated"/> moves the status back to
/// screening and, like any move back, leaves the stages already reached alone.
/// </para>
/// </summary>
public sealed class ApplicationReportProjection
    : EfCoreSingleStreamProjection<ApplicationReportRow, Guid, ApplicationsReportDbContext>
{
    public override ApplicationReportRow? ApplyEvent(
        ApplicationReportRow? snapshot,
        Guid identity,
        IEvent @event,
        ApplicationsReportDbContext dbContext,
        IQuerySession session
    )
    {
        switch (@event.Data)
        {
            case JobApplicationCreated created:
                return new ApplicationReportRow
                {
                    Id = created.JobApplicationId,
                    OrganizationId = created.OrganizationId,
                    JobPostId = created.JobPostId,
                    Source = created.Source.ToString(),
                    Status = nameof(JobApplicationStatus.Applied),
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.CreatedAt,
                };

            case JobApplicationScreeningStarted e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Screening, e.OccurredAt);

            case JobApplicationInterviewScheduled e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Interview, e.OccurredAt);

            case JobApplicationAssessmentStarted e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Assessment, e.OccurredAt);

            case JobApplicationOfferMade e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Offer, e.OccurredAt);

            case JobApplicationHired e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Hired, e.OccurredAt);

            case JobApplicationRejected e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Rejected, e.OccurredAt);

            case JobApplicationWithdrawn e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Withdrawn, e.OccurredAt);

            case JobApplicationReactivated e when snapshot is not null:
                return Reach(snapshot, JobApplicationStatus.Screening, e.OccurredAt);

            case JobApplicationStatusChanged e when snapshot is not null:
                return Reach(snapshot, e.NewStatus, e.OccurredAt);

            case not null when snapshot is not null:
                return Touch(snapshot, @event.Timestamp);

            default:
                return snapshot;
        }
    }

    private static ApplicationReportRow Reach(
        ApplicationReportRow row,
        JobApplicationStatus status,
        DateTimeOffset at
    )
    {
        row.Status = status.ToString();

        switch (status)
        {
            case JobApplicationStatus.Screening:
                row.ScreeningAt ??= at;
                break;
            case JobApplicationStatus.Interview:
                row.InterviewAt ??= at;
                break;
            case JobApplicationStatus.Assessment:
                row.AssessmentAt ??= at;
                break;
            case JobApplicationStatus.Offer:
                row.OfferAt ??= at;
                break;
            case JobApplicationStatus.Hired:
                row.HiredAt ??= at;
                break;
            case JobApplicationStatus.Rejected:
                row.RejectedAt ??= at;
                break;
            case JobApplicationStatus.Withdrawn:
                row.WithdrawnAt ??= at;
                break;
        }

        return Touch(row, at);
    }

    private static ApplicationReportRow Touch(ApplicationReportRow row, DateTimeOffset at)
    {
        if (at > row.UpdatedAt)
        {
            row.UpdatedAt = at;
        }

        return row;
    }
}
