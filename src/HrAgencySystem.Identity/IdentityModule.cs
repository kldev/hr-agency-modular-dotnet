using HrAgencySystem.Identity.Adapter;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Identity;

public static class IdentityModule
{
    public static void AddIdentityModule(
        this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();
    }
    
    public static void ConfigureMarten(
        StoreOptions options)
    {
        // options.Schema.For<UserEmailReservation>().DatabaseSchemaName("identity")
        //     .Index(
        //         x => new
        //         {
        //             x.OrganizationId,
        //             x.Email
        //         },
        //         idx => { idx.IsUnique = true; });

        options.Events.StreamIdentity =
            StreamIdentity.AsGuid;


        options.Events.AddEventType<UserCreated>();
        options.Events.AddEventType<PlatformOwnerCreated>();

        options.Schema.For<UserProjection>().DatabaseSchemaName("identity")
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, x.Email, x.Id });

        options.Schema.For<OwnerProjection>().DatabaseSchemaName("identity")
            .Index(x => new { x.Email });

    }
}