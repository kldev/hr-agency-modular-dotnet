using HrAgencySystem.Api.Endpoints;
using HrAgencySystem.Api.Infrastructure;
using HrAgencySystem.PlatformSeeder;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication();
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "HR Agency System";
            document.Info.Version = "v1";

            return Task.CompletedTask;
        });
    });
    builder.Services.SetupAppAuthorization(builder.Configuration);
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddPlatformSeederModule();
    }
}

var app = builder.Build();
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();
    app.MapApplicationEndpoints();
    app.MapOpenApi().AllowAnonymous();
    app.MapAppScalar();
    app.MapGet("/", () => "HR Agency API").ExcludeFromDescription().AllowAnonymous();

    app.Run();
}