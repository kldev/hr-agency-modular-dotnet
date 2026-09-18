using System.Diagnostics;
using HrAgencySystem.PlatformSeeder.Scenario;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HrAgencyShowcaseSeeder(
    IMessageBus bus,
    IDocumentSession session,
    ILogger<HrAgency.HrAgencyShowcaseSeeder> logger
) : IPlatformSeeder
{
    public async Task Seed()
    {
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Starting HR Agency showcase seeding");

        var owner = await new OwnerScenario(bus).Create();

        logger.LogInformation("Platform owner created: {PlatformOwnerId}", owner.PlatformOwnerId);

        await SeedAgency(owner.PlatformOwnerId, new SeedConfig());

        await SeedAgency(
            owner.PlatformOwnerId,
            new SeedConfig(
                Name: "Flex Jobs",
                Slug: "flex-jobs",
                UsersCount: 50,
                CompaniesCount: 999
            )
        );

        await SeedMinimalAgency(
            owner.PlatformOwnerId,
            new SeedConfig(Name: "Tech Jobs", Slug: "tech-jobs", UsersCount: 5, CompaniesCount: 20)
        );

        logger.LogInformation(
            "HR Agency showcase seeding completed in {Elapsed}",
            stopwatch.Elapsed
        );
    }

    private sealed record SeedConfig(
        string Name = "Hr Agency",
        string Slug = "hr-agency",
        int UsersCount = 20,
        int CompaniesCount = 101
    );
}
