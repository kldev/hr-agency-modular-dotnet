using System.Text.Json.Serialization;
using HrAgencySystem.Company;
using HrAgencySystem.Company.Infrastructure;
using HrAgencySystem.Organization;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Api.Infrastructure;

public static class SetupApplicationModulesExtensions
{
    extension(IServiceCollection services)
    {
        public void SetupApplicationModules()
        {
            services.ConfigureJson();
            services.AddTransient<IClock, SystemClock>();
            services.AddCompanyModule();
            services.AddOrganizationModule();
        }

        private void ConfigureJson()
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        }
    }
}