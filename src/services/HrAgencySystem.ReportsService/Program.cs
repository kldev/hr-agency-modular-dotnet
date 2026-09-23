using HrAgencySystem.ReportsService.Application.Platform;
using HrAgencySystem.ReportsService.Application.Recruitment;
using HrAgencySystem.ReportsService.Endpoints;
using HrAgencySystem.ReportsService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.SetupReportsDatabase(builder.Configuration);
    builder.Services.SetupServiceAuthorization(builder.Configuration);
    builder.Services.AddScoped<RecruitmentReportQuery>();
    builder.Services.AddScoped<PlatformReportQuery>();
    builder.Services.AddExceptionHandler<ServiceExceptionHandler>();
    builder.Services.AddProblemDetails();
}

var app = builder.Build();
{
    app.UseExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapReportEndpoints();

    app.Run();
}
