using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.PlatformSeeder;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication();
    builder.Services.AddOpenApi();
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddPlatformSeederModule();
    }
}

var app = builder.Build();
{
    app.UseExceptionHandler();
    app.MapApplicationEndpoints();
    app.MapOpenApi();
    app.MapAppScalar();
    app.MapGet("/", () => "HR Agency API").ExcludeFromDescription();

    app.Run();
}