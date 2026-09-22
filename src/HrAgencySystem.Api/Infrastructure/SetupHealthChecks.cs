using HrAgencySystem.Api.Infrastructure.FileServiceClient;
using HrAgencySystem.EmailTemplates.Messaging;
using HrAgencySystem.Feeds;
using HrAgencySystem.Files.Service;
using HrAgencySystem.Observability.Health;

namespace HrAgencySystem.Api.Infrastructure;

public static class SetupHealthChecks
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// What <c>/health/ready</c> asks. The file service is degraded rather than unhealthy when it
        /// is gone: the API still serves everything except documents, and an orchestrator should not
        /// pull a whole API node out of rotation for that.
        /// </summary>
        public void AddApiHealthChecks(IConfiguration configuration)
        {
            services
                .AddHealthChecks()
                .AddNpgSql(name: "postgres", tags: HealthTags.ReadyOnly, timeout: TimeSpan.FromSeconds(5))
                .AddRabbitMq(
                    RabbitMqConfig.FromSection(configuration.GetSection(RabbitMqConfig.SectionName)),
                    HealthTags.ReadyOnly
                )
                .AddObjectStorage(FeedBuckets.Jobs, HealthTags.ReadyOnly)
                .AddCheck<FileServiceHealthCheck>("file-service", tags: HealthTags.ReadyOnly);
        }
    }
}
