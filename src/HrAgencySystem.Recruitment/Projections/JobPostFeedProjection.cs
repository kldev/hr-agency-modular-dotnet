using HrAgencySystem.Feeds.ReadModel;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Recruitment.Projections;

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
                return MapCreated(created);

            case JobPostUpdated updated when snapshot is not null:
                return MapUpdated(snapshot, updated);

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

    /*
     * Mapping lives here, not on JobPostFeedRow: the row belongs to HrAgencySystem.Feeds, which
     * knows nothing about job post events. The Map* names are not decoration either - Marten treats
     * Create/Apply/ShouldDelete on the projection or on the projected document as conventional
     * handlers and refuses to combine them with the ApplyEvent override used here.
     */
    private static JobPostFeedRow MapCreated(JobPostCreated @event)
    {
        return new JobPostFeedRow
        {
            Id = @event.JobPostId,
            OrganizationId = @event.OrganizationId,
            IsPublished = false,
            Title = @event.Title,
            Summary = @event.Summary,
            Description = @event.Description,
            Responsibilities = [.. @event.Responsibilities],
            Requirements = [.. @event.Requirements],
            Skills = [.. @event.Skills],
            Location = @event.Location,
            LanguageCode = @event.LanguageCode,
            CountryCode = @event.CountryCode,
            EmploymentType = @event.EmploymentType,
            WorkMode = @event.WorkMode,
            CurrencyCode = @event.CurrencyCode,
            SalaryMin = @event.SalaryMin,
            SalaryMax = @event.SalaryMax,
            PostingSlug = @event.OrgSlug + "/" + @event.PostingSlug,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.CreatedAt
        };
    }

    private static JobPostFeedRow MapUpdated(JobPostFeedRow row, JobPostUpdated @event)
    {
        row.Title = @event.Title;
        row.Summary = @event.Summary ?? "";
        row.Description = @event.Description;
        row.Responsibilities = [.. @event.Responsibilities];
        row.Requirements = [.. @event.Requirements];
        row.Skills = [.. @event.Skills];
        row.Location = @event.Location;
        row.LanguageCode = @event.LanguageCode;
        row.CountryCode = @event.CountryCode;
        row.EmploymentType = @event.EmploymentType;
        row.WorkMode = @event.WorkMode;
        row.CurrencyCode = @event.CurrencyCode;
        row.SalaryMin = @event.SalaryMin;
        row.SalaryMax = @event.SalaryMax;
        row.UpdatedAt = @event.OccurredAt;

        return row;
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
