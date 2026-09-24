using System.Text.Json.Serialization;
using HrAgencySystem.Agency;
using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.Api.Infrastructure.ReportsClient;
using HrAgencySystem.Company;
using HrAgencySystem.Feeds;
using HrAgencySystem.Files;
using HrAgencySystem.Forms;
using HrAgencySystem.Identity;
using HrAgencySystem.JobDescription;
using HrAgencySystem.LegalEntities;
using HrAgencySystem.Organization;
using HrAgencySystem.Projects;
using HrAgencySystem.Recruitment;
using HrAgencySystem.Sales;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks;
using HrAgencySystem.Teams;
using HrAgencySystem.Workers;

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
            services.AddReportsClient(configuration);
            services.AddAgencyModule();
            services.AddProjectsModule();
            services.AddWorkersModule();
            services.AddFormsModule();
            services.AddSalesModule();
            services.AddTasksModule();
            services.AddTeamsModule();
            services.AddLegalEntitiesModule();
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
