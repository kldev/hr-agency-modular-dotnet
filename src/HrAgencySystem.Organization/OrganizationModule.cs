using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.Organization.Infrastructure;
using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Port;
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
    }
    
    public static void ConfigureMarten(
        StoreOptions options)
    {
        ConfigureTable(options);
        ConfigureEvents(options);
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
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        options.Events.AddEventType(
            typeof(OrganizationCreated));
    }
}