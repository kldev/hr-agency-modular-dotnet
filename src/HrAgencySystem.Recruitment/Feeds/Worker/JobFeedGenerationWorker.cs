using HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Recruitment.Feeds.Worker;

public sealed class JobFeedGenerationWorker(IServiceScopeFactory scopeFactory, 
    ILogger<IJobFeedProcessor> logger) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(120));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IJobFeedProcessor>();
                await service.ProcessBatch(stoppingToken);

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