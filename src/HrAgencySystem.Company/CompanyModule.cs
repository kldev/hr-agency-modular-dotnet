using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Infrastructure.Persistence;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Company;

public static class CompanyModule
{
    public static void AddCompanyModule(
        this IServiceCollection services)
    {
        services.AddScoped<ICompanyTaxIdReservationRepository, CompanyTaxIdReservationRepository>();
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
    }
}