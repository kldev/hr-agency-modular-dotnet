using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.Organization.Infrastructure;
using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.Organization.Projections;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Services;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Organization;

public static class OrganizationModule
{
    public static void AddOrganizationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOrganizationSlugReservationRepository, OrganizationSlugReservationRepository>();
        services.AddScoped<IOrganizationChecker, OrganizationChecker>();
        services.AddScoped<IQueryOrganizationRepository, QueryOrganizationRepository>();
        services.AddScoped<IOrganizationQueryRepository, OrganizationQueryRepository>();
    }
    
    public static void ConfigureMarten(
        StoreOptions options)
    {
        ConfigureTable(options);
        ConfigureEvents(options);
        ConfigureProjections(options);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Snapshot<OrganizationProjection>(SnapshotLifecycle.Inline);
    }

    private static void ConfigureTable(StoreOptions options)
    {
        options.Schema.For<OrganizationSlugReservation>().DatabaseSchemaName("organization")
            .Index(
                x => new
                {
                    x.Slug
                },
                idx => { idx.IsUnique = true; });

        options.Schema.For<OrganizationProjection>().DatabaseSchemaName("organization")
            .Index(z => z.Name)
            .Index(z => z.Slug)
            .Index(z => z.CreatedAt);
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        options.Events.AddEventType(
            typeof(OrganizationCreated));
        options.Events.AddEventType<OrganizationUpdated>();
        options.Events.AddEventType<OrganizationSlugUpdated>();
    }
}