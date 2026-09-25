using System.Diagnostics;
using HrAgencySystem.PlatformSeeder.Scenario;
using HrAgencySystem.Sales.Services;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HrAgencyShowcaseSeeder(
    IMessageBus bus,
    IDocumentSession session,
    ISalesService sales,
    ILogger<HrAgencyShowcaseSeeder> logger
) : IPlatformSeeder
{
    /// <summary>The one agency that gets projects, workers and assignments.</summary>
    private const string DeliverySlug = "hr-agency";

    public async Task Seed()
    {
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Starting HR Agency showcase seeding");

        var owner = await new OwnerScenario(bus, session).Create();

        logger.LogInformation("Platform owner created: {PlatformOwnerId}", owner.PlatformOwnerId);

        await new ServiceApiKeyScenario(session).Create(owner.PlatformOwnerId);

        logger.LogInformation("Job board service key seeded");

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

        // Last, and against one named agency rather than inside SeedAgency: this is the slowest
        // scenario and the one most likely to trip over a domain rule, and neither is a reason for
        // the other two agencies to end up half seeded.
        await SeedDelivery(DeliverySlug);

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
