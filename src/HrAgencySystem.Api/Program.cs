using HrAgencySystem.Api;
using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.Api.Infrastructure.ReportsClient;
using HrAgencySystem.Observability.AspNetCore;
using HrAgencySystem.PlatformSeeder;
using Microsoft.Extensions.Options;

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
    // Configuration before the database: a wrong secret or url stops the host naming the setting,
    // instead of seeding first and failing later for a reason that looks unrelated.
    app.Services.GetRequiredService<IStartupValidator>().Validate();
    await app.SeedAsync();

    app.UseRequestLogging();
    // Right after the request log, so the log still sees the final status, and before
    // authentication: the api key handler reads the database, and a failure there deserves
    // ProblemDetails with a trace id as much as one inside an endpoint.
    app.UseExceptionHandler();
    app.UseCors();
    app.UseAuthentication();
    app.UseTenantTelemetry();
    app.UseAuthorization();
    app.MapApplicationEndpoints();
    app.MapOpenApi().AllowAnonymous();
    app.MapAppScalar();
    app.MapHealthEndpoints();
    app.MapGet(ApiEndpoints.Root, () => "HR Agency API").ExcludeFromDescription().AllowAnonymous();
    app.MapGet(
            ApiEndpoints.Health,
            async (
                FileServiceHealthProbe files,
                ReportsHealthProbe reports,
                CancellationToken ct
            ) =>
                new
                {
                    status = "UP",
                    fileService = await files.CheckAsync(ct),
                    reports = await reports.CheckAsync(ct),
                }
        )
        .ExcludeFromDescription()
        .AllowAnonymous();

    app.Logger.LogInformation(
        "HR agency API starting in {Environment}",
        app.Environment.EnvironmentName
    );

    await app.RunAsync();
}
