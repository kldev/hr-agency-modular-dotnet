using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Application.Query;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Company.Projections;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Company;

public static class CompanyModule
{
    public static void AddCompanyModule(
        this IServiceCollection services)
    {
        services.AddScoped<ICompanyTaxIdReservationRepository, CompanyTaxIdReservationRepository>();
        services.AddScoped<ICompaniesQueryRepository, CompaniesQueryRepository>();
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        options.Schema.For<CompanyTaxIdReservation>().DatabaseSchemaName("company")
            .Index(
                x => new
                {
                    x.OrganizationId,
                    x.TaxId
                },
                idx => { idx.IsUnique = true; });

        options.Events.StreamIdentity =
            StreamIdentity.AsGuid;


        options.Events.AddEventType<CompanyCreated>();
        options.Projections.Snapshot<CompanyProjection>(SnapshotLifecycle.Async);

        options.Schema.For<CompanyProjection>().DatabaseSchemaName("company")
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, x.Name, x.Id });
    }
}