using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication();
    //builder.Services.AddWolverineHttp();
    builder.Services.AddOpenApi();
    
}

var app = builder.Build();
{
    app.UseExceptionHandler();
    app.MapApplicationEndpoints();
    app.MapOpenApi();
    app.MapAppScalar();
    //app.MapWolverineEndpoints();

    app.MapGet("/", () => "HR Agency API").ExcludeFromDescription();

    app.Run();
}