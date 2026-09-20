using HrAgencySystem.Api;
using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.PlatformSeeder;
using JasperFx;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.AddDataSource();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication(builder.Configuration);
    builder.Services.AddAppOpenApi();
    builder.Services.SetupAppAuthorization(builder.Configuration, builder.Environment);
    if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "docker")
    {
        builder.Services.AddPlatformSeederModule();
    }

    builder.Host.ApplyJasperFxExtensions();
}

var app = builder.Build();
{
    await app.SeedAsync();

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();
    app.MapApplicationEndpoints();
    app.MapOpenApi().AllowAnonymous();
    app.MapAppScalar();
    app.MapGet(ApiEndpoints.Root, () => "HR Agency API").ExcludeFromDescription().AllowAnonymous();
    app.MapGet(
            ApiEndpoints.Health,
            async (FileServiceHealthProbe files, CancellationToken ct) =>
                new { status = "UP", fileService = await files.CheckAsync(ct) }
        )
        .ExcludeFromDescription()
        .AllowAnonymous();

    Console.WriteLine("HR agency API started");
    Console.WriteLine("Environment: " + app.Environment.EnvironmentName);

    await app.RunJasperFxCommands(args);
}
