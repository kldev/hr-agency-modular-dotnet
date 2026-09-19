using HrAgencySystem.EmailTemplates.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.EmailTemplates;

public static class EmailTemplatesModule
{
    extension(IServiceCollection services)
    {
        public void AddEMailTemplates()
        {
            services.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();
        }
    }
}
