using HrAgencySystem.Identity.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Identity;

public static class IdentityModule
{
    public static void AddIdentityModule(this IServiceCollection services)
    {
        services.AddIdentityServices();
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
