using System.Text.Json.Serialization;
using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.Company;
using HrAgencySystem.Company.Infrastructure;
using HrAgencySystem.Feeds;
using HrAgencySystem.Files;
using HrAgencySystem.Identity;
using HrAgencySystem.JobDescription;
using HrAgencySystem.Organization;
using HrAgencySystem.Recruitment;
using HrAgencySystem.Sales;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams;

namespace HrAgencySystem.Api.Infrastructure;

public static class SetupApplicationModulesExtensions
{
    extension(IServiceCollection services)
    {
        public void SetupApplicationModules(IConfiguration configuration)
        {
            services.AddOptions<ApplicationConfig>();
            services.Configure<ApplicationConfig>(
                configuration.GetSection(ApplicationConfig.Section)
            );

            services.ConfigureJson();
            services.AddTransient<IClock, SystemClock>();
            services.AddCompanyModule();
            services.AddOrganizationModule(configuration);
            services.AddIdentityModule(configuration);
            services.AddJobDescriptionModule();
            services.AddRecruitmentModule(configuration);
            services.AddFeedsModule(configuration);
            services.AddFilesModule(configuration);
            services.AddFileServiceClient(configuration);
            services.AddSalesModule();
            services.AddTeamsModule();
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
