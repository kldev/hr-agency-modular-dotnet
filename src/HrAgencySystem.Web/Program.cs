using HrAgencySystem.Web.Endpoints;
using HrAgencySystem.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGlobalExceptionHandler();
    builder.Services.SetupApplicationModules(builder.Configuration);
    builder.Services.SetupMartenForApplication(builder.Configuration);
    builder.Host.SetupWolverineForApplication();
    builder.Services.AddRazorPages();
}

var app = builder.Build();
{
    
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
    app.MapRazorPages()
        .WithStaticAssets();
    app.MapGet("/", () => "HR Agency Web").ExcludeFromDescription();
    app.Run();
}