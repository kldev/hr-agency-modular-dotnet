using HrAgencySystem.Api.Common.Errors;

namespace HrAgencySystem.Api.Infrastructure;

public static class MapProblemDetailsExtensions
{
    public static void AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}