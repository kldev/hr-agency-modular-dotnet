using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.PlatformSeeder;
using JasperFx;
using JasperFx.Events;

var builder = WebApplication.CreateBuilder(args);
{
    var environmentName = builder.Environment.EnvironmentName;
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.AddDataSource();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication();
    builder.Services.AddAppOpenApi();
    builder.Services.SetupAppAuthorization(builder.Configuration,builder.Environment);
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
    app.MapGet("/", () => "HR Agency API").ExcludeFromDescription().AllowAnonymous();
    app.MapGet("/healthz", () => new { status = "UP" }).ExcludeFromDescription().AllowAnonymous();

    Console.WriteLine("HR agency API started");
    Console.WriteLine("Environment: " + app.Environment.EnvironmentName);
    
    await app.RunJasperFxCommands(args);
}