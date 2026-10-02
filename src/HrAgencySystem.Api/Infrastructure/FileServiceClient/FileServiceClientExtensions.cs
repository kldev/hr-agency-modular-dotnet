using HrAgencySystem.FileService.Contracts;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

public static class FileServiceClientExtensions
{
    extension(IServiceCollection services)
    {
        public void AddFileServiceClient(IConfiguration configuration)
        {
            services
                .AddOptions<FileServiceClientConfig>()
                .Bind(configuration.GetSection(FileServiceClientConfig.SectionName))
                .ValidateBaseUrl(FileServiceClientConfig.SectionName, config => config.BaseUrl)
                .ValidateServiceSecret(FileServiceClientConfig.SectionName, config => config.Secret)
                .Validate(
                    config => config.TimeoutSeconds > 0,
                    $"{FileServiceClientConfig.SectionName}:TimeoutSeconds must be positive."
                )
                .ValidateOnStart();

            services.AddSingleton<FileServiceTokenFactory>();
            services.AddSingleton<FileServiceHealthProbe>();
            // No retry handler on purpose: the calls that matter here carry a request body stream,
            // and a stream cannot be replayed. Retrying an upload would send an empty file.
            services.AddHttpClient<IFileServiceClient, HttpFileServiceClient>(
                (provider, client) =>
                {
                    var config = provider.GetRequiredService<IOptions<FileServiceClientConfig>>();
                    client.BaseAddress = new Uri(config.Value.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(config.Value.TimeoutSeconds);
                }
            );
        }
    }
}
