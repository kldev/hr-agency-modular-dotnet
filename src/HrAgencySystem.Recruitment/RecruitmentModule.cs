using HrAgencySystem.Recruitment.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Recruitment;

public static class RecruitmentModule
{
    public static void AddRecruitmentModule(this IServiceCollection services)
    {
        services.AddRecruitmentServices();
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureRecruitmentDocuments();
        options.ConfigureRecruitmentEvents();
        options.ConfigureRecruitmentProjections();
    }
}
