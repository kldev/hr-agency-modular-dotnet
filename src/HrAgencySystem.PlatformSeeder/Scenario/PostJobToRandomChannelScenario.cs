using HrAgencySystem.Recruitment.Application.JobPosting.PostToChannel;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPosting;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

public sealed class PostJobToRandomChannelScenario(IMessageBus bus, IQuerySession session)
{
    private static readonly Random Random = new();
    
    public async Task Execute(IReadOnlyList<Guid> ids)
    {
        var jobs = await session.Query<JobPostCreated>().ToListAsync();
        foreach (var job in jobs)
        {
            var randomUser = ids[Random.Next(0, ids.Count)];
            await bus.InvokeAsync(new PostToChannel(job.JobPostId, job.OrganizationId,RandomChannel(), randomUser));
        }

        var i = 0;
        while (i++ < 30)
        {
            var job = jobs[Random.Next(0, jobs.Count)];
            var randomUser = ids[Random.Next(0, ids.Count)];
            await bus.InvokeAsync(new PostToChannel(job.JobPostId, job.OrganizationId,RandomChannel(), randomUser));
        }
    }
    
    private static PostingChannelType RandomChannel()
    {
        var values = Enum.GetValues<PostingChannelType>();
        return values[Random.Next(values.Length)];
    }
}