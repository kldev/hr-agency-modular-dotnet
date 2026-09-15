using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Add;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Events.Applications;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class TagsScenario(IMessageBus bus, IDocumentSession session)
{
    internal async Task Seed(Guid organizationId, IReadOnlyList<Guid> userIds)
    {
        IEnumerable<string> postTitles = ["C# Developer", "NextJS Developer", "NodeJS Developer"];
        var applicantsIds = await session
            .Query<JobApplicationCreated>()
            .Where(z => z.OrganizationId == organizationId)
            .Where(z => postTitles.Contains(z.JobPostTitle))
            .Select(z=>z.JobApplicationId)
            .ToListAsync();

        var tags = await session.Query<Tag>()
            .Where(z=>z.Category == TagCategory.SkillIt).ToListAsync();

        foreach (var applicantId in applicantsIds)
        {
            await TagApplicant(applicantId,  organizationId, tags, userIds);
        }
    }

    private async Task TagApplicant(Guid applicantId, Guid organizationId, IReadOnlyList<Tag> tags, IReadOnlyList<Guid> userIds)
    {
        var selectedTags = RandomTags(tags);
        var commands = 
            selectedTags.Select(z => 
                new TagApplication(z.Id, applicantId, organizationId, userIds[Random.Shared.Next(userIds.Count)]));

        foreach (var command in commands)
        {
            await bus.InvokeAsync<JobApplicationTagged>(command);
        }
    }
    
    
    private IReadOnlyList<Tag> RandomTags(IReadOnlyList<Tag> tags) {
        var count = Random.Shared.Next(1, 10);
        List<Tag> selectedTag = [];

        while (selectedTag.Count < count)
        {
            var pull = tags.Where(z => !selectedTag.Contains(z)).ToList();
            selectedTag.Add(pull[Random.Shared.Next(pull.Count)]);
        }

        return selectedTag;
    }
    
}