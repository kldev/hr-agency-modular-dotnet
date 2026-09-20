using System.Text;
using HrAgencySystem.Feeds.Model;
using HrAgencySystem.Feeds.Port;
using HrAgencySystem.Files;
using HrAgencySystem.Files.Model;
using HrAgencySystem.Files.Service;

namespace HrAgencySystem.Feeds.Application.GenerateJobFeed;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class JobFeedProcessor(
    IJobFeedTaskQueue fetcher,
    IJobFeedTaskRepository repository,
    IObjectStorage objectStorage,
    IJobFeedGenerator generator
) : IJobFeedProcessor
{
    public async Task ProcessBatch(CancellationToken ct)
    {
        var tasks = await fetcher.Fetch(100, ct);

        foreach (var task in tasks)
        {
            try
            {
                await ProcessTask(task, ct);
            }
            catch (Exception ex)
            {
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

        await objectStorage.StoreAsync(
            new FileInput(streamJson, "jobs.json", "application/json"),
            task.OrganizationId + "/jobs.json",
            BucketNames.FeedJobs,
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

        await objectStorage.StoreAsync(
            new FileInput(stream, "jobs.xml", "application/xml"),
            task.OrganizationId + "/jobs.xml",
            BucketNames.FeedJobs,
            ct
        );
    }
}
