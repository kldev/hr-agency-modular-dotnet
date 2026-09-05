using HrAgencySystem.Files.Config;
using HrAgencySystem.Files.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Files;

public static class FilesModule
{
    public static void AddFilesModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(S3Config.SectionName);
        services.Configure<S3Config>(ops =>
        {
            ops.AccessKey = section[nameof(S3Config.AccessKey)] ?? "";
            ops.SecretKey = section[nameof(S3Config.SecretKey)] ?? "";
            ops.Endpoint = section[nameof(S3Config.Endpoint)] ?? "";
        });
        services.AddSingleton<IFileStorage, RustFsS3Service>();
    }
}