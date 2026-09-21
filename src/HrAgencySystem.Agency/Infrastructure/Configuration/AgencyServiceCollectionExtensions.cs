using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Infrastructure.Query;
using HrAgencySystem.Agency.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Agency.Infrastructure.Configuration;

internal static class AgencyServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddAgencyServices()
        {
            services.AddScoped<IAgencyService, AgencyService>();
            services.AddScoped<IOrgStructureQueryRepository, OrgStructureQueryRepository>();
        }
    }
}
