using HrAgencySystem.Files.Config;
using HrAgencySystem.Files.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Files;

public static class FilesModule
{
    extension(IServiceCollection services)
    {
        public void AddFilesModule(IConfiguration configuration)
        {
            services
                .AddOptions<S3Config>()
                .Bind(configuration.GetSection(S3Config.SectionName))
                .Validate(
                    config =>
                        Uri.TryCreate(config.Endpoint, UriKind.Absolute, out var uri)
                        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                    $"{S3Config.SectionName}:Endpoint must be an absolute http(s) url."
                )
                .Validate(
                    config =>
                        !string.IsNullOrWhiteSpace(config.AccessKey)
                        && !string.IsNullOrWhiteSpace(config.SecretKey),
                    $"{S3Config.SectionName}:AccessKey and {S3Config.SectionName}:SecretKey are required."
                )
                .ValidateOnStart();
            services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        }
    }
}
