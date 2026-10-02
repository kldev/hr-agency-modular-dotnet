using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.EmailTemplates;

public static class EmailTemplatesModule
{
    private const string ProviderKey = "MailProvider";
    private const string MailKitProvider = "mailkit";

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

            services
                .AddOptions<SmtpConfig>()
                .Bind(configuration.GetSection(SmtpConfig.SectionName))
                .Validate(
                    config =>
                        !string.IsNullOrWhiteSpace(config.Host)
                        && config.Port is > 0 and <= 65535
                        && !string.IsNullOrWhiteSpace(config.FromEmail),
                    $"{SmtpConfig.SectionName}:Host, a port between 1 and 65535 and "
                        + $"{SmtpConfig.SectionName}:FromEmail are required with the mailkit provider."
                )
                .ValidateOnStart();
            services.AddScoped<ISendEmail, MailKitEmailSender>();
        }
    }
}
