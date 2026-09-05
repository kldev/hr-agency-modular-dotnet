using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.PlatformSeeder;
using JasperFx;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.AddDataSource();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication();
    builder.Services.AddAppOpenApi();
    builder.Services.SetupAppAuthorization(builder.Configuration);
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddPlatformSeederModule();
    }
    
    builder.Host.ApplyJasperFxExtensions();
}

var app = builder.Build();
{
    await app.SeedAsync();
    
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();
    app.MapApplicationEndpoints();
    app.MapOpenApi().AllowAnonymous();
    app.MapAppScalar();
    app.MapGet("/", () => "HR Agency API").ExcludeFromDescription().AllowAnonymous();

    await app.RunJasperFxCommands(args);
}