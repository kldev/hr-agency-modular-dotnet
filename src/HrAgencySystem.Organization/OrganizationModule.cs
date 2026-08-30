using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.Organization.Infrastructure.Persistence;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Organization;

public static class OrganizationModule
{
    public static void AddOrganizationModule(this IServiceCollection services)
    {
        services.AddScoped<IOrganizationSlugReservationRepository, OrganizationSlugReservationRepository>();
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        options.Schema.For<OrganizationSlugReservation>().DatabaseSchemaName("org")
            .Index(
                x => new
                {
                    x.Slug
                },
                idx => { idx.IsUnique = true; });


        options.Events.AddEventType(
            typeof(OrganizationCreated));
    }
}