using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
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
        options.Events.AddEventType<Events.JobApplication.JobApplicationCreated>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationAssessmentStarted>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationHired>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationInterviewScheduled>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationOfferMade>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationRejected>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationScreeningStarted>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationWithdrawn>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationTagged>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationTagRemoved>();
        
        // Job Posting
        options.Events.AddEventType<Events.JobPosting.JobPostCreated>();
        options.Events.AddEventType<Events.JobPosting.JobPostUpdated>();
        options.Events.AddEventType<Events.JobPosting.JobPostToChannel>();
        options.Events.AddEventType<Events.JobPosting.JobPostPublished>();
        options.Events.AddEventType<Events.JobPosting.JobPostClosed>();
        options.Events.AddEventType<Events.JobPosting.JobPostArchived>();
        
        // Candidate
        options.Events.AddEventType<Events.Candidate.CandidateCreated>();
        options.Events.AddEventType<Events.Candidate.CandidateApplicationUpdated>();
        options.Events.AddEventType<Events.Candidate.CandidateTagged>();
        options.Events.AddEventType<Events.Candidate.CandidateTagRemoved>();
        
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
            .Index(x => new { OrganizationId = x.OrgId, x.Source });;
        
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