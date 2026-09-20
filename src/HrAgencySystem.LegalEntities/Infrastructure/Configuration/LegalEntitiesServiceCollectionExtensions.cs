using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Infrastructure.Persistence;
using HrAgencySystem.LegalEntities.Infrastructure.Query;
using HrAgencySystem.LegalEntities.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.LegalEntities.Infrastructure.Configuration;

internal static class LegalEntitiesServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddLegalEntitiesServices()
        {
            services.AddScoped<ILegalEntitiesQueryRepository, LegalEntitiesQueryRepository>();
            services.AddScoped<ILegalEntitiesService, LegalEntitiesService>();
            services.AddScoped<ILegalEntitySnapshotRepository, LegalEntitySnapshotRepository>();
            services.AddScoped<
                ILegalEntityTaxIdReservationRepository,
                LegalEntityTaxIdReservationRepository
            >();
        }
    }
}
