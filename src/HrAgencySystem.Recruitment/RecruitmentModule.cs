using HrAgencySystem.Recruitment.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Recruitment;

public static class RecruitmentModule
{
     private const string SchemaName = "recruitment";

    public static void AddRecruitmentModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRecruitmentServices(configuration);
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        options.ConfigureRecruitmentDocuments();
        options.ConfigureRecruitmentEvents();
        options.ConfigureRecruitmentProjections();
    }
}