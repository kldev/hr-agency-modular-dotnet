using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Add;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Events.Applications;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class TagsScenario(IMessageBus bus, IDocumentSession session, ILogger logger)
{
    private int TagAddCount { get; set; } = 0;

    internal async Task Seed(Guid organizationId, IReadOnlyList<Guid> userIds)
    {
        IEnumerable<string> postTitles =
        [
            "C# Developer",
            "NextJS Developer",
            "NodeJS Developer",
            "Java Backend Entwickler",
            "ava Backend Developer",
        ];
        var applicants = await session
            .Query<JobApplicationCreated>()
            .Where(z => z.OrganizationId == organizationId)
            .Where(z => postTitles.Contains(z.JobPostTitle))
            .ToListAsync();

        var tags = await session
            .Query<Tag>()
            .Where(z => z.Category == TagCategory.SkillIt)
            .ToListAsync();

        foreach (var applicant in applicants)
        {
            await TagApplicant(applicant.JobApplicationId, organizationId, tags, userIds);
        }

        logger.LogInformation("Seeded {TagCount} application tags", TagAddCount);
    }

    private async Task TagApplicant(
        Guid applicantId,
        Guid organizationId,
        IReadOnlyList<Tag> tags,
        IReadOnlyList<Guid> userIds
    )
    {
        var selectedTags = RandomTags(tags);
        var commands = selectedTags.Select(z => new TagApplication(
            z.Id,
            applicantId,
            organizationId,
            userIds[Random.Shared.Next(userIds.Count)]
        ));

        foreach (var command in commands)
        {
            await bus.InvokeAsync<JobApplicationTagged>(command);
            TagAddCount++;
        }
    }

    private IReadOnlyList<Tag> RandomTags(IReadOnlyList<Tag> tags)
    {
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
