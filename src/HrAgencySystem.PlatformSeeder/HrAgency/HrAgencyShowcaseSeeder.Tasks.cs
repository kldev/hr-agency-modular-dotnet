using HrAgencySystem.PlatformSeeder.Scenario;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

public sealed partial class HrAgencyShowcaseSeeder
{
    private async Task PostToChannel(IReadOnlyList<Guid> userIds)
    {
        logger.LogDebug("Posting jobs to random channels for {UserCount} users", userIds.Count);

        await new PostJobToRandomChannelScenario(bus, session).Execute(userIds);
    }

    private async Task GenerateApplicants(int count = 500, bool includeShowcase = false)
    {
        logger.LogDebug("Generating {ApplicantCount} applicants", count);

        await new ApplyToJobPostScenario(bus, session).Execute(count, includeShowcase);
    }
}
