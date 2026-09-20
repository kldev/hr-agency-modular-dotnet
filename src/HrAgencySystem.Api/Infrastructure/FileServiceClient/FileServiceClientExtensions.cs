using HrAgencySystem.FileService.Contracts;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

public static class FileServiceClientExtensions
{
    extension(IServiceCollection services)
    {
        public void AddFileServiceClient(IConfiguration configuration)
        {
            var section = configuration.GetSection(FileServiceClientConfig.SectionName);
            services.Configure<FileServiceClientConfig>(section);

            var config = section.Get<FileServiceClientConfig>() ?? new FileServiceClientConfig();

            services.AddSingleton<FileServiceTokenFactory>();
            services.AddSingleton<FileServiceHealthProbe>();
            // No retry handler on purpose: the calls that matter here carry a request body stream,
            // and a stream cannot be replayed. Retrying an upload would send an empty file.
            services.AddHttpClient<IFileServiceClient, HttpFileServiceClient>(client =>
            {
                client.BaseAddress = new Uri(
                    string.IsNullOrWhiteSpace(config.BaseUrl)
                        ? "http://localhost:5100"
                        : config.BaseUrl
                );
                client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            });
        }
    }
}
