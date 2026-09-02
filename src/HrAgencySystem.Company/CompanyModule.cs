using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Application.Query;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Company.Infrastructure.Snapshots;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Company;

public static class CompanyModule
{
    private const string SchemaName = "company";
    public static void AddCompanyModule(
        this IServiceCollection services)
    {
        services.AddScoped<ICompanyTaxIdReservationRepository, CompanyTaxIdReservationRepository>();
        services.AddScoped<ICompaniesQueryRepository, CompaniesQueryRepository>();
        services.AddScoped<ICompanySnapshotService, CompanySnapshotService>();
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
        options.Schema.For<CompanyTaxIdReservation>().DatabaseSchemaName(SchemaName)
            .Index(
                x => new
                {
                    x.OrganizationId,
                    x.TaxId
                },
                idx => { idx.IsUnique = true; });
    }

    private static void ConfigureEvents(StoreOptions options)
    {
        options.Events.AddEventType<CompanyCreated>();
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Snapshot<CompanyProjection>(SnapshotLifecycle.Async);

        options.Schema.For<CompanyProjection>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, x.Name, x.Id })
            .Index(x => new { x.OrganizationId, x.CreatedId });
    }
}