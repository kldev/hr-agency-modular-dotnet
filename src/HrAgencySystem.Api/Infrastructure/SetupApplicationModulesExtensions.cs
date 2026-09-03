using System.Text.Json.Serialization;
using HrAgencySystem.Company;
using HrAgencySystem.Company.Infrastructure;
using HrAgencySystem.Identity;
using HrAgencySystem.JobDescription;
using HrAgencySystem.Organization;
using HrAgencySystem.Recruitment;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Api.Infrastructure;

public static class SetupApplicationModulesExtensions
{
    extension(IServiceCollection services)
    {
        public void SetupApplicationModules(IConfiguration configuration)
        {
            services.ConfigureJson();
            services.AddTransient<IClock, SystemClock>();
            services.AddCompanyModule();
            services.AddOrganizationModule(configuration);
            services.AddIdentityModule();
            services.AddJobDescriptionModule();
            services.AddRecruitmentModule();
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