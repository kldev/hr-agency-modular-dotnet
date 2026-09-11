using HrAgencySystem.Recruitment.Projections;
using JasperFx.Events.Projections;
using Marten;

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
        }

        public void ConfigureRecruitmentProjectionsMinimal()
        {
            ConfigureJobPostProjection(options, true);
            ConfigureCandidateProjection(options, true);
        }
    }

    private static void ConfigureJobApplicationProjection(
        StoreOptions options)
    {
        options.Projections.Snapshot<JobApplicationProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<JobApplicationProjection>()
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

    private static void ConfigureJobPostProjection(
        StoreOptions options, bool skipSnapshots = false)
    {
        if (!skipSnapshots)
        {
            options.Projections.Snapshot<JobPostProjection>(
                SnapshotLifecycle.Async);
        }

        options.Schema
            .For<JobPostProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.CompanyId })
            .Index(x => new { x.OrgId, x.Status })
            .Index(x => new { x.OrgId, x.LanguageCode })
            .Index(x => new { x.OrgId, x.RecruiterId })
            .Index(x => new { x.OrgId, x.Title, x.Id })
            .Index(x => new { x.OrgId, x.Company.Name })
            .Index(x => new { x.OrgId, x.Company.TaxId })
            .Index(x => new { x.OrgId, x.SearchText });
    }

    private static void ConfigureCandidateProjection(
        StoreOptions options, bool skipSnapshots = false)
    {
        if (!skipSnapshots)
        {
            options.Projections.Snapshot<CandidateProjection>(
                SnapshotLifecycle.Async);
        }

        options.Schema
            .For<CandidateProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.Email })
            .Index(x => new { x.OrgId, x.PhoneNumber })
            .Index(x => new { x.OrgId, x.CreatedAt });
    }

    private static void ConfigureInterviewProjection(
        StoreOptions options)
    {

        options.Projections.Snapshot<InterviewProjection>(
            SnapshotLifecycle.Async);


        options.Schema
            .For<InterviewProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.ScheduleAt })
            .Index(x => new { x.OrgId, InterviewId = x.Id })
            .Index(x => new { x.OrgId, x.ApplicationId })
            .Index(x => new { x.OrgId, x.CreatedByUserId })
            .Index(x => new { x.OrgId, x.CreatedAt })
            .Index(x => new
            {
                x.OrgId,
                x.CreatedAt, 
                x.Format, 
                x.Status, 
                x.ApplicantInfo.Email,
                x.ApplicantInfo.FirstName,
                x.ApplicantInfo.LastName,
                x.ApplicantInfo.PhoneNumber
            }, idx => { idx.Name = "idx_interview_search"; });
    }
}