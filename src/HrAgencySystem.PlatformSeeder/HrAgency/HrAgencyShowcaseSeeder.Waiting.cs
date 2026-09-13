using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

public sealed partial class HrAgencyShowcaseSeeder
{
    private const int ProjectionDelayMs = 5_000;

    private async Task WaitForProjections(int times = 1)
    {
        LoggerExtensions.LogDebug(logger, "Waiting {DelayMs}ms for projections ({Times} times)",
            ProjectionDelayMs,
            times);

        await Task.Delay(ProjectionDelayMs * times);
    }
}