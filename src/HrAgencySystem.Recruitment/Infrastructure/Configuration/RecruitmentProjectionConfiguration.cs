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
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.CompanyId })
            .Index(x => new { OrganizationId = x.OrgId, x.TagsIds })
            .Index(x => new { OrganizationId = x.OrgId, x.ApplicantEmail })
            .Index(x => new { OrganizationId = x.OrgId, x.ApplicantPhone })
            .Index(x => new { OrganizationId = x.OrgId, x.ApplicantFullName })
            .Index(x => new { OrganizationId = x.OrgId, x.JobPostTitle })
            .Index(x => new { OrganizationId = x.OrgId, x.Source });
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
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.CompanyId })
            .Index(x => new { OrganizationId = x.OrgId, x.Status })
            .Index(x => new { OrganizationId = x.OrgId, x.LanguageCode })
            .Index(x => new { OrganizationId = x.OrgId, x.RecruiterId })
            .Index(x => new { OrganizationId = x.OrgId, x.Title, x.Id })
            .Index(x => new { OrganizationId = x.OrgId, x.Company.Name })
            .Index(x => new { OrganizationId = x.OrgId, x.Company.TaxId })
            .Index(x => new { OrganizationId = x.OrgId, x.SearchText });
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
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.Email })
            .Index(x => new { OrganizationId = x.OrgId, x.PhoneNumber })
            .Index(x => new { OrganizationId = x.OrgId, x.CreatedAt });
    }

    private static void ConfigureInterviewProjection(
        StoreOptions options)
    {

        options.Projections.Snapshot<InterviewProjection>(
            SnapshotLifecycle.Async);


        options.Schema
            .For<InterviewProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.ScheduleAt })
            .Index(x => new { OrganizationId = x.OrgId, InterviewId = x.Id })
            .Index(x => new { OrganizationId = x.OrgId, x.ApplicationId })
            .Index(x => new { OrganizationId = x.OrgId, x.CreatedByUserId })
            .Index(x => new { OrganizationId = x.OrgId, x.CreatedAt });
    }
}