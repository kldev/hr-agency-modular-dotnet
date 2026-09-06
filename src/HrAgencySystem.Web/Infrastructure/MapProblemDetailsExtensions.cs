using HrAgencySystem.Web.Common.Errors;

namespace HrAgencySystem.Web.Infrastructure;

public static class MapProblemDetailsExtensions
{
    extension(IServiceCollection services)
    {
        public void AddGlobalExceptionHandler()
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails(); 
        }
    }
    
}