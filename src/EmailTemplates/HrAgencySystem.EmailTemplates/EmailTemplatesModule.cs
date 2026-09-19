using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.EmailTemplates;

public static class EmailTemplatesModule
{
    public const string ProviderKey = "MailProvider";
    public const string MailKitProvider = "mailkit";

    extension(IServiceCollection services)
    {
        public void AddEMailTemplates(IConfiguration configuration)
        {
            services.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();

            var provider = configuration[ProviderKey];

            if (!string.Equals(provider, MailKitProvider, StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<ISendEmail, LoggingEmailSender>();
                return;
            }

            services.Configure<SmtpConfig>(configuration.GetSection(SmtpConfig.SectionName));
            services.AddScoped<ISendEmail, MailKitEmailSender>();
        }
    }
}
