using HrAgencySystem.Api;
using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.Observability.AspNetCore;
using HrAgencySystem.PlatformSeeder;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddWebObservability("hr-api");
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.AddDataSource();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication(builder.Configuration);
    builder.Services.AddAppOpenApi();
    builder.Services.AddApiHealthChecks(builder.Configuration);
    builder.Services.SetupAppAuthorization(builder.Configuration, builder.Environment);
    if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "docker")
    {
        builder.Services.AddPlatformSeederModule();
    }
}

var app = builder.Build();
{
    await app.SeedAsync();

    app.UseRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseTenantTelemetry();
    app.UseAuthorization();
    app.UseExceptionHandler();
    app.MapApplicationEndpoints();
    app.MapOpenApi().AllowAnonymous();
    app.MapAppScalar();
    app.MapHealthEndpoints();
    app.MapGet(ApiEndpoints.Root, () => "HR Agency API").ExcludeFromDescription().AllowAnonymous();
    app.MapGet(
            ApiEndpoints.Health,
            async (FileServiceHealthProbe files, CancellationToken ct) =>
                new { status = "UP", fileService = await files.CheckAsync(ct) }
        )
        .ExcludeFromDescription()
        .AllowAnonymous();

    app.Logger.LogInformation(
        "HR agency API starting in {Environment}",
        app.Environment.EnvironmentName
    );

    await app.RunAsync();
}
