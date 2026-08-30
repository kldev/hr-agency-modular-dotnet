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
        var disableCheck = configuration.GetValue<bool>("Application:DisableChecker");

        SetupChecker(services, disableCheck);
    }

    private static void SetupChecker(IServiceCollection services, bool disableCheck)
    {
        if (disableCheck)
        {
            services.AddScoped<IOrganizationChecker, TestOrganizationChecker>();
        }
        else
        { 
            services.AddScoped<IOrganizationChecker, OrganizationChecker>();
        }
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