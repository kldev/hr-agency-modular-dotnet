using HrAgencySystem.Agency.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Agency;

/// <summary>
/// The agency as an employer: its own shape and, in time, its own people's leave and hours.
/// <para>
/// Separate from <c>Organization</c> although both are about the same real company. Organization is
/// the tenant - the row that scopes every other module's data and answers "does this id exist".
/// This one is the company's internal life, and the two change for entirely different reasons: a
/// tenant gains a slug, an agency gains a department.
/// </para>
/// <para>
/// Separate from <c>Company</c> for the plainer reason: Company is the clients.
/// </para>
/// </summary>
public static class AgencyModule
{
    extension(IServiceCollection services)
    {
        public void AddAgencyModule()
        {
            services.AddAgencyServices();
        }
    }

    // No Minimal variant: the public job board shows offers to candidates, and a candidate never
    // sees who reports to whom.
    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
