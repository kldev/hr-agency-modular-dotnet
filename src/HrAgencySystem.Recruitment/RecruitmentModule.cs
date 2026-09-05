using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Feeds.Port;
using HrAgencySystem.Recruitment.Feeds.Repository;
using HrAgencySystem.Recruitment.Feeds.Table;
using HrAgencySystem.Recruitment.Infrastructure;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
using HrAgencySystem.Recruitment.Infrastructure.Query;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Port;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Recruitment;

public static class RecruitmentModule
{
     private const string SchemaName = "recruitment";

    public static void AddRecruitmentModule(
        this IServiceCollection services)
    {
        services.AddScoped<ISeeder, TagSeeder>();
        services.AddScoped<IJobPostQueryRepository,JobPostQueryRepository>();
        services.AddScoped<ITagSuggestionRepository, TagSuggestionRepository>();
        services.AddScoped<ICandidateEmailReservationRepository, CandidateEmailReservationRepository>();
        services.AddScoped<ICandidateQueryRepository, CandidateQueryRepository>();
        services.AddScoped<ICandidateResolver, CandidateResolver> ();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IJobApplicationQueryRepository, JobApplicationQueryRepository>();
        services.AddScoped<ISeeder, FeedMigration>();
        services.AddScoped<IJobFeedTaskRepository, JobFeedTaskRepository>();
        services.AddScoped<IJobFeedTaskBatchFetcher, JobFeedTaskBatchFetcher>();
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        ConfigureTable(options);
        ConfigureEvents(options);
        ConfigureProjections(options);
    }

    private static void ConfigureTable(StoreOptions options)
    {
        options.Schema.For<Tag>().DatabaseSchemaName(SchemaName)
            .Index(z => z.Category)
            .Index(z => new { z.Category, z.Code }, idx => { idx.IsUnique = true; })
            .Index(z => z.Name);

        options.Schema.For<JobApplicationNote>().DatabaseSchemaName(SchemaName)
            .Index(
                x => new { x.Id, x.OrgId },
                idx => { idx.IsUnique = true; })
            .Index(z => new { z.OrgId, z.JobApplicationId });

        options.Schema.For<CandidateEmailReservation>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.Email, x.OrganizationId }, idx => { idx.IsUnique = true; });
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        // Job Application
        options.Events.AddEventType<JobApplicationCreated>();
        options.Events.AddEventType<JobApplicationAssessmentStarted>();
        options.Events.AddEventType<JobApplicationHired>();
        options.Events.AddEventType<JobApplicationInterviewScheduled>();
        options.Events.AddEventType<JobApplicationOfferMade>();
        options.Events.AddEventType<JobApplicationRejected>();
        options.Events.AddEventType<JobApplicationScreeningStarted>();
        options.Events.AddEventType<JobApplicationWithdrawn>();
        options.Events.AddEventType<JobApplicationTagged>();
        options.Events.AddEventType<JobApplicationTagRemoved>();
        
        // Job Posting
        options.Events.AddEventType<JobPostCreated>();
        options.Events.AddEventType<JobPostUpdated>();
        options.Events.AddEventType<JobPostedToChannel>();
        options.Events.AddEventType<JobPostPublished>();
        options.Events.AddEventType<JobPostClosed>();
        options.Events.AddEventType<JobPostArchived>();
        
        // Candidate
        options.Events.AddEventType<CandidateCreated>();
        options.Events.AddEventType<CandidateApplicationUpdated>();
        options.Events.AddEventType<CandidateTagged>();
        options.Events.AddEventType<CandidateTagRemoved>();
        
    }

    private static void ConfigureProjections(StoreOptions options)
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
        
        options.Projections.Snapshot<JobPostProjection>(
            SnapshotLifecycle.Async);

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
        
        options.Projections.Snapshot<CandidateProjection>(
            SnapshotLifecycle.Async);
        
        options.Schema
            .For<CandidateProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.Email })
            .Index(x => new { OrganizationId = x.OrgId, x.PhoneNumber })
            .Index(x => new { OrganizationId = x.OrgId, x.CreatedAt });
        
    }
}