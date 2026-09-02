using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Infrastructure;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Infrastructure.Snapshots;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Identity;

public static class IdentityModule
{
    private const string SchemaName = "identity";
    public static void AddIdentityModule(
        this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IUserEmailReservationRepository, UserEmailReservationRepository>();
        services.AddScoped<IOwnerEmailReservationRepository, OwnerEmailReservationRepository>();
        services.AddScoped<IUserSnapshotService, UserSnapshotService>();
        services.AddTransient<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddOptions<JwtConfig>(JwtConfig.Section);
    }
    
    public static void ConfigureMarten(
        StoreOptions options)
    {
        ConfigureTable(options);
        ConfigureEvents(options);
        ConfigureProjections(options);
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        options.Events.AddEventType<UserCreated>();
        options.Events.AddEventType<PlatformOwnerCreated>();
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Snapshot<UserProjection>(SnapshotLifecycle.Async);
        
        options.Schema.For<UserProjection>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, x.Email, x.Id });

        options.Projections.Snapshot<OwnerProjection>(SnapshotLifecycle.Async);
        options.Schema.For<OwnerProjection>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.Email });
    }
    

    private static void ConfigureTable(StoreOptions options)
    {
        options.Schema.For<UserEmailReservation>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId, x.Email },
                idx => { idx.IsUnique = true; });
        
        options.Schema.For<OwnerEmailReservation>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.Email },
                idx => { idx.IsUnique = true; });
    }
}