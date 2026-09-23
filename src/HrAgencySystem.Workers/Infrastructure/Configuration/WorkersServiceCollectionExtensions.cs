using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Application.Suggestion;
using HrAgencySystem.Workers.Infrastructure.Persistence;
using HrAgencySystem.Workers.Infrastructure.Query;
using HrAgencySystem.Workers.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Workers.Infrastructure.Configuration;

internal static class WorkersServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddWorkersServices()
        {
            services.AddScoped<IWorkersQueryRepository, WorkersQueryRepository>();
            services.AddScoped<IAssignmentsQueryRepository, AssignmentsQueryRepository>();
            services.AddScoped<IWorkerSuggestionRepository, WorkerSuggestionRepository>();
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IWorkerSnapshotRepository, WorkerSnapshotRepository>();
            services.AddScoped<IWorkersService, WorkersService>();
            services.AddScoped<
                IWorkerIdentityDocumentReservationRepository,
                WorkerIdentityDocumentReservationRepository
            >();
            services.AddScoped<
                IWorkerEmailReservationRepository,
                WorkerEmailReservationRepository
            >();
        }
    }
}
