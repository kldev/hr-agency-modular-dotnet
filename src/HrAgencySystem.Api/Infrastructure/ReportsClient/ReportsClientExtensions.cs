using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

public static class ReportsClientExtensions
{
    extension(IServiceCollection services)
    {
        public void AddReportsClient(IConfiguration configuration)
        {
            services
                .AddOptions<ReportsClientConfig>()
                .Bind(configuration.GetSection(ReportsClientConfig.SectionName))
                .ValidateBaseUrl(ReportsClientConfig.SectionName, config => config.BaseUrl)
                .ValidateServiceSecret(ReportsClientConfig.SectionName, config => config.Secret)
                .Validate<IOptions<FileServiceClientConfig>>(
                    (config, files) => config.Secret != files.Value.Secret,
                    $"{ReportsClientConfig.SectionName}:Secret must differ from "
                        + $"{FileServiceClientConfig.SectionName}:Secret."
                )
                .Validate(
                    config => config.TimeoutSeconds > 0,
                    $"{ReportsClientConfig.SectionName}:TimeoutSeconds must be positive."
                )
                .ValidateOnStart();

            services.AddSingleton<ReportsTokenFactory>();
            services.AddSingleton<ReportsHealthProbe>();
            services.AddHttpClient<IReportsClient, HttpReportsClient>(
                (provider, client) =>
                {
                    var config = provider.GetRequiredService<IOptions<ReportsClientConfig>>();
                    client.BaseAddress = new Uri(config.Value.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(config.Value.TimeoutSeconds);
                }
            );
        }
    }
}
