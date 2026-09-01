using HrAgencySystem.JobDescription.Events;

using HrAgencySystem.JobDescription.Projections;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.JobDescription;

public static class JobDescriptionModule
{
    private const string SchemaName = "job_description";

    public static void AddJobDescriptionModule(
        this IServiceCollection services)
    {
       // services.AddScoped<IJobDescriptionsQueryRepository, JobDescriptionsQueryRepository>();
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        ConfigureEvents(options);
        ConfigureProjections(options);
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        options.Events.AddEventType<JobDescriptionCreated>();
        options.Events.AddEventType<JobDescriptionUpdated>();
        options.Events.AddEventType<JobDescriptionOpened>();
        options.Events.AddEventType<JobDescriptionPutOnHold>();
        options.Events.AddEventType<JobDescriptionClosed>();
        options.Events.AddEventType<JobDescriptionCancelled>();
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Snapshot<JobDescriptionProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<JobDescriptionProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, x.CompanyId })
            .Index(x => new { x.OrganizationId, x.Status })
            .Index(x => new { x.OrganizationId, x.RecruiterId })
            .Index(x => new { x.OrganizationId, x.Title, x.Id });
    }
}