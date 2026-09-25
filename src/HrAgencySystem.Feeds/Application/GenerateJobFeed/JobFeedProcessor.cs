using System.Diagnostics;
using System.Text;
using HrAgencySystem.Feeds.Model;
using HrAgencySystem.Feeds.Port;
using HrAgencySystem.Feeds.Telemetry;
using HrAgencySystem.Files.Model;
using HrAgencySystem.Files.Service;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Feeds.Application.GenerateJobFeed;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class JobFeedProcessor(
    IJobFeedTaskQueue fetcher,
    IJobFeedTaskRepository repository,
    IObjectStorage objectStorage,
    IJobFeedGenerator generator,
    FeedTelemetry telemetry,
    ILogger<JobFeedProcessor> logger
) : IJobFeedProcessor
{
    public async Task ProcessBatch(CancellationToken ct)
    {
        var tasks = await fetcher.Fetch(100, ct);

        foreach (var task in tasks)
        {
            using var activity = FeedTelemetry.Source.StartActivity("generate job feed");
            activity?.SetTag("hr.organization_id", task.OrganizationId);
            var started = Stopwatch.GetTimestamp();

            try
            {
                await ProcessTask(task, ct);
                telemetry.RecordGeneration(FeedTelemetry.Completed, Stopwatch.GetElapsedTime(started));
            }
            catch (Exception ex)
            {
                // The task row keeps the message, but nobody reads that table - without this line a
                // feed that stopped updating would leave no trace anywhere a person looks.
                logger.LogError(
                    ex,
                    "Generating the job feed of organization {OrganizationId} failed",
                    task.OrganizationId
                );
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                telemetry.RecordGeneration(FeedTelemetry.Failed, Stopwatch.GetElapsedTime(started));
                await repository.MarkFailed(task.Id, ex.Message, ct);
            }
        }
    }

    private async Task ProcessTask(JobFeedTask task, CancellationToken ct)
    {
        var result = await generator.GenerateAsync(task.OrganizationId, ct);

        await StoreXml(task, ct, result);

        await StoreJson(task, ct, result);

        await repository.MarkCompleted(task.Id, ct);
    }

    private async Task StoreJson(
        JobFeedTask task,
        CancellationToken ct,
        JobFeedGenerator.JobFeedContent result
    )
    {
        await using var streamJson = new MemoryStream(Encoding.UTF8.GetBytes(result.Json));
        telemetry.RecordSize("json", streamJson.Length);

        await objectStorage.StoreAsync(
            new FileInput(streamJson, "jobs.json", "application/json"),
            task.OrganizationId + "/jobs.json",
            FeedBuckets.Jobs,
            ct
        );
    }

    private async Task StoreXml(
        JobFeedTask task,
        CancellationToken ct,
        JobFeedGenerator.JobFeedContent result
    )
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(result.Xml));
        telemetry.RecordSize("xml", stream.Length);

        await objectStorage.StoreAsync(
            new FileInput(stream, "jobs.xml", "application/xml"),
            task.OrganizationId + "/jobs.xml",
            FeedBuckets.Jobs,
            ct
        );
    }
}
