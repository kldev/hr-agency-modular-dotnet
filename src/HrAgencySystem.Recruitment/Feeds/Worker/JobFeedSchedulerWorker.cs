using HrAgencySystem.Recruitment.Feeds.Application.ScheduleFeedTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Recruitment.Feeds.Worker;

internal sealed class JobFeedSchedulerWorker(IServiceScopeFactory scopeFactory, 
    ILogger<IJobFeedScheduler> logger) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IJobFeedScheduler>();


                await service.ScheduleAsync(stoppingToken);

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to create job feed tasks.");
            }
        }
    }
}