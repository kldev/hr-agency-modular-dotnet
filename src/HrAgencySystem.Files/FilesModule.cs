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
            services.Configure<S3Config>(configuration.GetSection(S3Config.SectionName));
            services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        }
    }
}
