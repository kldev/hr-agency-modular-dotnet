using HrAgencySystem.Observability.AspNetCore;
using HrAgencySystem.Observability.Health;
using HrAgencySystem.ReportsService.Application.Platform;
using HrAgencySystem.ReportsService.Application.Recruitment;
using HrAgencySystem.ReportsService.Endpoints;
using HrAgencySystem.ReportsService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddWebObservability("hr-reports-service");

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.SetupReportsDatabase(builder.Configuration);
    builder.Services.SetupServiceAuthorization(builder.Configuration);
    builder.Services.AddScoped<RecruitmentReportQuery>();
    builder.Services.AddScoped<PlatformReportQuery>();
    builder.Services.AddExceptionHandler<ServiceExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder
        .Services.AddHealthChecks()
        .AddNpgSql(name: "postgres", tags: HealthTags.ReadyOnly, timeout: TimeSpan.FromSeconds(5));
}

var app = builder.Build();
{
    app.UseRequestLogging();
    app.UseExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapReportEndpoints();
    app.MapHealthEndpoints();

    app.Run();
}
