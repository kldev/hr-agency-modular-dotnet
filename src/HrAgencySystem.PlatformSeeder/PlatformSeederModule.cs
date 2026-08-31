using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.PlatformSeeder;

public static class PlatformSeederModule
{
    public static void AddPlatformSeederModule(this IServiceCollection services)
    {
        services.AddScoped<IPlatformSeeder, HrAgencyShowcaseSeeder>();
    }
}