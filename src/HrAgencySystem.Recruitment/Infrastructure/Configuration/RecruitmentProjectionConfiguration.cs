using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.Recruitment.Projections.Timeline;
using JasperFx.Events.Projections;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Recruitment.Infrastructure.Configuration;

internal static class RecruitmentProjectionConfiguration
{
    private const string SchemaName = "recruitment";

    extension(StoreOptions options)
    {
        public void ConfigureRecruitmentProjections()
        {
            ConfigureJobApplicationProjection(options);
            ConfigureJobPostProjection(options);
            ConfigureCandidateProjection(options);
            ConfigureInterviewProjection(options);
            ConfigureJobPostFeedProjection(options);
            ConfigureTimelineProjection(options);
        }
    }

    /*
     * One row per event of the candidate, application and interview streams. Both indexes end
     * in (OccurredAt, Sequence), the keyset the timeline pages by.
     */
    private static void ConfigureTimelineProjection(StoreOptions options)
    {
        options.Projections.Add(new TimelineProjection(), ProjectionLifecycle.Async);

        options
            .Schema.For<TimelineEntry>()
            .DatabaseSchemaName(SchemaName)
            .Index(
                x => new
                {
                    x.OrgId,
                    x.CandidateId,
                    x.OccurredAt,
                    x.Sequence,
                },
                idx =>
                {
                    idx.Name = "idx_timeline_candidate";
                }
            )
            .Index(
                x => new
                {
                    x.OrgId,
                    x.JobApplicationId,
                    x.OccurredAt,
                    x.Sequence,
                },
                idx =>
                {
                    idx.Name = "idx_timeline_application";
                }
            );
    }

    /*
     * Feed read model: the relational table the feed worker serializes from.
     */
    private static void ConfigureJobPostFeedProjection(StoreOptions options)
    {
        options.Add(new JobPostFeedProjection(), ProjectionLifecycle.Async);
    }

    private static void ConfigureJobApplicationProjection(StoreOptions options)
    {
        options.Projections.Snapshot<JobApplicationProjection>(SnapshotLifecycle.Async);

        options
            .Schema.For<JobApplicationProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.CompanyId })
            .Index(x => new { x.OrgId, x.TagsIds })
            .Index(x => new { x.OrgId, x.ApplicantEmail })
            .Index(x => new { x.OrgId, x.ApplicantPhone })
            .Index(x => new { x.OrgId, x.ApplicantFullName })
            .Index(x => new { x.OrgId, x.JobPostTitle })
            .Index(x => new { x.OrgId, x.Source });
    }

    private static void ConfigureJobPostProjection(StoreOptions options)
    {
        options.Projections.Snapshot<JobPostProjection>(SnapshotLifecycle.Async);

        options
            .Schema.For<JobPostProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.CompanyId })
            .Index(x => new { x.OrgId, x.Status })
            .Index(x => new { x.OrgId, x.LanguageCode })
            .Index(x => new { x.OrgId, x.RecruiterId })
            .Index(x => new
            {
                x.OrgId,
                x.Title,
                x.Id,
            })
            .Index(x => new { x.OrgId, x.Company.Name })
            .Index(x => new { x.OrgId, x.Company.TaxId })
            .Index(x => new { x.OrgId, x.SearchText });
    }

    private static void ConfigureCandidateProjection(StoreOptions options)
    {
        options.Projections.Snapshot<CandidateProjection>(SnapshotLifecycle.Async);

        options
            .Schema.For<CandidateProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.Email })
            .Index(x => new { x.OrgId, x.PhoneNumber })
            .Index(x => new { x.OrgId, x.CreatedAt });
    }

    private static void ConfigureInterviewProjection(StoreOptions options)
    {
        options.Projections.Snapshot<InterviewProjection>(SnapshotLifecycle.Async);

        options
            .Schema.For<InterviewProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.ScheduleAt })
            .Index(x => new { x.OrgId, InterviewId = x.Id })
            .Index(x => new { x.OrgId, x.ApplicationId })
            .Index(x => new { x.OrgId, x.CreatedByUserId })
            .Index(x => new { x.OrgId, x.CreatedAt })
            .Index(
                x => new
                {
                    x.OrgId,
                    x.CreatedAt,
                    x.Format,
                    x.Status,
                    x.ApplicantInfo.Email,
                    x.ApplicantInfo.FirstName,
                    x.ApplicantInfo.LastName,
                    x.ApplicantInfo.PhoneNumber,
                },
                idx =>
                {
                    idx.Name = "idx_interview_search";
                }
            );
    }
}
