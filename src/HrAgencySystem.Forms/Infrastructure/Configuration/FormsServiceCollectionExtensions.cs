using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Infrastructure.Persistence;
using HrAgencySystem.Forms.Infrastructure.Query;
using HrAgencySystem.Forms.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Forms.Infrastructure.Configuration;

internal static class FormsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddFormsServices()
        {
            services.AddScoped<IFormsService, FormsService>();
            services.AddScoped<IFormsRepository, FormsRepository>();
            services.AddScoped<IFormCodeReservationRepository, FormCodeReservationRepository>();
            services.AddScoped<IFormsQueryRepository, FormsQueryRepository>();
            services.AddScoped<IFormResponsesQueryRepository, FormResponsesQueryRepository>();
        }
    }
}
