using HrAgencySystem.Feeds.Application.GenerateJobFeed;
using HrAgencySystem.Feeds.Application.GetJobFeed;
using HrAgencySystem.Feeds.Application.ScheduleFeedTasks;
using HrAgencySystem.Feeds.Config;
using HrAgencySystem.Feeds.Persistence;
using HrAgencySystem.Feeds.Port;
using HrAgencySystem.Feeds.Telemetry;
using HrAgencySystem.Feeds.Worker;
using HrAgencySystem.SharedKernel.Port;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Feeds;

public static class FeedsModule
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Everything needed to read the feed read model, build the files and store them.
        /// Registered by every host that owns the feed schema; running the generation on a schedule
        /// is a separate decision - see <see cref="AddFeedsBackgroundWorkers"/>.
        /// </summary>
        public void AddFeedsModule(IConfiguration configuration)
        {
            var section = configuration.GetSection(FeedsConfig.Section);

            services.Configure<FeedsConfig>(options =>
            {
                options.FeedUrl = section[nameof(FeedsConfig.FeedUrl)] ?? string.Empty;
            });

            services.AddScoped<ISeeder, FeedMigration>();

            services.AddScoped<IJobFeedTaskRepository, JobFeedTaskRepository>();
            services.AddScoped<IJobFeedTaskQueue, JobFeedTaskQueue>();
            services.AddScoped<IJobFeedScheduler, JobFeedScheduler>();
            services.AddScoped<IJobFeedProcessor, JobFeedProcessor>();
            services.AddSingleton<FeedTelemetry>();
            services.AddScoped<IJobFeedGenerator, JobFeedGenerator>();
            services.AddScoped<IJobFeedReader, JobFeedReader>();
        }

        /// <summary>
        /// The two hosted services that actually produce the files. Only the feeds worker host
        /// registers them - an API replica that happens to serve HTTP has no business generating
        /// feeds, and several schedulers would only fight over the same task queue.
        /// </summary>
        public void AddFeedsBackgroundWorkers()
        {
            services.AddHostedService<JobFeedSchedulerWorker>();
            services.AddHostedService<JobFeedGenerationWorker>();
        }
    }
}
