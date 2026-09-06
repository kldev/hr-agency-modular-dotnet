using System.Text.Json.Serialization;
using HrAgencySystem.Company;
using HrAgencySystem.Files;
using HrAgencySystem.Organization;
using HrAgencySystem.Recruitment;
using HrAgencySystem.Recruitment.Infrastructure.Configuration;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Web.Config;
using HrAgencySystem.Web.Services;

namespace HrAgencySystem.Web.Infrastructure;

public static class SetupApplicationModulesExtensions
{
    extension(IServiceCollection services)
    {
        public void SetupApplicationModules(IConfiguration configuration)
        {
            services.AddOptions<ApplicationConfig>();
            services.Configure<ApplicationConfig>(
                configuration.GetSection(ApplicationConfig.Section));
            services.ConfigureJson();
            services.AddTransient<IClock, SystemClock>();

            services.AddScoped<IUserSnapshotRepository, WebUserSnapshotRepository>();
            
            services.AddOrganizationModule(configuration);
            services.AddRecruitmentModuleMinimal();
            services.AddCompanyMinimalModule();
            services.AddFilesModule(configuration);
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