using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Company.Infrastructure.Query;
using HrAgencySystem.Company.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Company.Infrastructure.Configuration;

internal static class CompanyServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddCompanyServices()
        {
            services.AddScoped<ICompanyTaxIdReservationRepository, CompanyTaxIdReservationRepository>();
            services.AddScoped<ICompaniesQueryRepository, CompaniesQueryRepository>();
            services.AddScoped<ICompanySnapshotRepository, CompanySnapshotRepository>();
            services.AddScoped<ICompanySuggestionRepository, CompanySuggestionRepository>();
            services.AddScoped<ICompanyContactQueryRepository, CompanyContactQueryRepository>();
            services.AddScoped<ICompanyContactSuggestionRepository, CompanyContactSuggestionRepository>();
            services.AddScoped<ICompanyContactRepository, CompanyContactRepository>();
            services.AddScoped<ICompanyService, CompanyService>();
        }
    }
}