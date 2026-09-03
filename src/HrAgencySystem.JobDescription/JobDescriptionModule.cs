using HrAgencySystem.JobDescription.Application.Port;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Infrastructure.Query;
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
       services.AddScoped<IJobDescriptionQueryRepository, JobDescriptionQueryRepository>();
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
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.CompanyId })
            .Index(x => new { OrganizationId = x.OrgId, x.Status })
            .Index(x => new { OrganizationId = x.OrgId, x.RecruiterId })
            .Index(x => new { OrganizationId = x.OrgId, x.Title, x.Id })
            .Index(x => new { OrganizationId = x.OrgId, x.Company.Name })
            .Index(x => new { OrganizationId = x.OrgId, x.Company.TaxId });

        
        options.Projections.Add(new StatusChangeHistoryProjection(),
            ProjectionLifecycle.Async);

        options.Schema
            .For<JdStatusChangeHistory>()
            .DatabaseSchemaName(SchemaName)
            .Index(z=> new { OrganizationId = z.OrgId })
            .Index(z=> new { OrganizationId = z.OrgId, JobDescriptionId = z.JobDescriptionId });
        
    }
}