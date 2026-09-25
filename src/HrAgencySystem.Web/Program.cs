using HrAgencySystem.Observability.AspNetCore;
using HrAgencySystem.Observability.Health;
using HrAgencySystem.Web.Endpoints;
using HrAgencySystem.Web.Infrastructure;
using HrAgencySystem.Web.Services;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddWebObservability("hr-web");
    builder.Services.AddGlobalExceptionHandler();

    // This host reads and writes through the API's internal routes only - no database, no bus.
    builder.Services.AddJobBoardClient(builder.Configuration);
    builder.Services.AddRazorPages();
    builder.Services.AddHealthChecks()
        .AddCheck<ApiHealthProbe>("api", tags: HealthTags.ReadyOnly);
}

var app = builder.Build();
{
    app.UseRequestLogging();
    app.MapApplicationEndpoints();
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseHttpsRedirection();

    app.UseRouting();

    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();
    app.MapHealthEndpoints();
    app.MapGet("/", () => "HR Agency Web").ExcludeFromDescription();
    app.Run();
}
