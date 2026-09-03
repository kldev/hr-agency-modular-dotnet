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
        options.Events.AddEventType<Events.JobApplication.JobApplicationCreated>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationAssessmentStarted>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationHired>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationInterviewScheduled>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationOfferMade>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationRejected>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationScreeningStarted>();
        options.Events.AddEventType<Events.JobApplication.JobApplicationWithdrawn>();
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Snapshot<JobApplicationProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<JobApplicationProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { OrganizationId = x.OrgId });
    }
}