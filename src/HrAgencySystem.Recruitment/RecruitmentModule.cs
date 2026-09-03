using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
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
        //services.AddScoped<IRecruitmentQueryRepository, RecruitmentQueryRepository>();
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
            .Index(z => new { z.Category, z.Code }, idx => { idx.IsUnique = true; });

        options.Schema.For<JobApplicationNote>().DatabaseSchemaName(SchemaName)
            .Index(
                x => new { x.Id, x.OrgId },
                idx => { idx.IsUnique = true; })
            .Index(z => new { z.OrgId, z.JobApplicationId });
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        // Job Application
        options.Events.AddEventType<Events.JobApplication.JobApplicationCreated>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationAssessmentStarted>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationHired>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationInterviewScheduled>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationOfferMade>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationRejected>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationScreeningStarted>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationWithdrawn>();
        
        // Job Posting
        options.Events.AddEventType<Events.JobPosting.JobPostCreated>();
        options.Events.AddEventType<Events.JobPosting.JobPostUpdated>();
        options.Events.AddEventType<Events.JobPosting.JobPostToChannel>();
        options.Events.AddEventType<Events.JobPosting.JobPostPublished>();
        options.Events.AddEventType<Events.JobPosting.JobPostClosed>();
        options.Events.AddEventType<Events.JobPosting.JobPostArchived>();
        
        // Candidate
        options.Events.AddEventType<Events.Candidate.CandidateCreated>();
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Snapshot<JobApplicationProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<JobApplicationProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { OrganizationId = x.OrgId });
        
        options.Projections.Snapshot<JobPostingProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<JobPostingProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { OrganizationId = x.OrganizationId })
            .Index(x => new { OrganizationId = x.OrganizationId, x.CompanyId })
            .Index(x => new { OrganizationId = x.OrganizationId, x.Status })
            .Index(x => new { OrganizationId = x.OrganizationId, x.LanguageCode })
            .Index(x => new { OrganizationId = x.OrganizationId, x.RecruiterId })
            .Index(x => new { OrganizationId = x.OrganizationId, x.Title, x.Id })
            .Index(x => new { OrganizationId = x.OrganizationId, x.Company.Name })
            .Index(x => new { OrganizationId = x.OrganizationId, x.Company.TaxId });
    }
}