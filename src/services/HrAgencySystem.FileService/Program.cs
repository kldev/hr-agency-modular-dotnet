using HrAgencySystem.Files;
using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Config;
using HrAgencySystem.FileService.Endpoints;
using HrAgencySystem.FileService.Infrastructure;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.Configure<FileServiceConfig>(
        builder.Configuration.GetSection(FileServiceConfig.SectionName)
    );

    var config =
        builder.Configuration.GetSection(FileServiceConfig.SectionName).Get<FileServiceConfig>()
        ?? new FileServiceConfig();

    builder.Services.Configure<FormOptions>(options =>
    {
        // The inspector refuses an oversized upload, but only after ASP.NET has agreed to read it.
        // Capping the form here is what stops a 2 GB body from ever reaching managed memory.
        options.MultipartBodyLengthLimit = config.MaxSizeBytes;
    });

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddFilesModule(builder.Configuration);
    builder.Services.SetupMartenForFileService(builder.Configuration);
    builder.Services.SetupServiceAuthorization(builder.Configuration);
    builder.Services.AddScoped<IUploadInspector, UploadInspector>();
    builder.Services.AddScoped<IFileStore, FileStore>();
    builder.Services.AddExceptionHandler<ServiceExceptionHandler>();
    builder.Services.AddProblemDetails();
}

var app = builder.Build();
{
    app.UseExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapFileEndpoints();

    app.Run();
}
