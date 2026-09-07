using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Company.Infrastructure.Query;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using HrAgencySystem.SharedKernel.Snapshots;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Company;

public static class CompanyModule
{
    private const string SchemaName = "company";
    extension(IServiceCollection services)
    {
        public void AddCompanyModule()
        {
            services.AddScoped<ICompanyTaxIdReservationRepository, CompanyTaxIdReservationRepository>();
            services.AddScoped<ICompaniesQueryRepository, CompaniesQueryRepository>();
            services.AddScoped<ICompanySnapshotRepository, CompanySnapshotRepository>();
            services.AddScoped<ICompanySuggestionRepository, CompanySuggestionRepository>();
        }
    }

    public static void AddCompanyMinimalModule(this IServiceCollection services)
    {
        services.AddScoped<ICompanySnapshotRepository, CompanySnapshotRepository>();
    }

    public static void ConfigureMartenMinimal(StoreOptions options)
    {
        ConfigureProjections(options);
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
        options.Events.AddEventType<CompanyJobPostCreated>();
        options.Events.AddEventType<CompanyJobPostActiveChanged>();
    }

    private static void ConfigureProjections(StoreOptions options, bool skipSnapshots = false)
    {
        if (!skipSnapshots)
        {
            options.Projections.Snapshot<CompanyProjection>(SnapshotLifecycle.Async);
        }

        options.Schema.For<CompanyProjection>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, x.Name })
            .Index(x => new { x.OrganizationId, x.CreatedId })
            .Index(x => new { x.OrganizationId, x.CountryCode });


    }
}