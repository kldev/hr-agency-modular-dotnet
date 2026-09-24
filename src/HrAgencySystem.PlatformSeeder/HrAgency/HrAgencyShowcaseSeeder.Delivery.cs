using HrAgencySystem.Company.Projections;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.Organization.Projections;
using HrAgencySystem.PlatformSeeder.Scenario;
using HrAgencySystem.Teams.Projections;
using HrAgencySystem.Workers.Projections;
using Marten;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.HrAgency;

public sealed partial class HrAgencyShowcaseSeeder
{
    /// <summary>
    /// Re-runs the delivery part against an agency the rest of the seed already built.
    /// </summary>
    public async Task SeedDelivery(string slug)
    {
        var organization = await session
            .Query<OrganizationProjection>()
            .FirstOrDefaultAsync(o => o.Slug == slug);

        if (organization is null)
            throw new InvalidOperationException($"No organization found for slug '{slug}'.");

        var userIds = await session
            .Query<UserProjection>()
            .Where(u => u.OrganizationId == organization.Id)
            .Select(u => u.Id)
            .ToListAsync();

        var companyIds = await session
            .Query<CompanyProjection>()
            .Where(c => c.OrganizationId == organization.Id)
            .Select(c => c.Id)
            .ToListAsync();

        if (userIds.Count == 0 || companyIds.Count == 0)
            throw new InvalidOperationException(
                $"Organization '{slug}' has no users or no companies to deliver for."
            );

        var teamIds = await session
            .Query<TeamProjection>()
            .Where(t => t.OrganizationId == organization.Id)
            .Select(t => t.Id)
            .ToListAsync();

        await SeedDelivery(
            new OrganizationScenario.OrganizationData(organization.Id, organization.Slug),
            userIds,
            companyIds,
            teamIds
        );
    }

    /// <summary>
    /// Everything downstream of a sale: our own companies, the projects they deliver, and the people
    /// on those projects. Only one agency gets this - it is the slowest part of the seed, and three
    /// copies of the same register would say nothing the first one does not.
    /// </summary>
    private async Task SeedDelivery(
        OrganizationScenario.OrganizationData organization,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds,
        IReadOnlyList<Guid> teamIds
    )
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        logger.LogInformation(
            "Seeding delivery for organization {OrganizationId} ({Slug})",
            organization.OrganizationId,
            organization.Slug
        );

        // Registering the same cast twice cannot work: identity documents and e-mail addresses are
        // reserved per organization, so a second run would die halfway through and leave the
        // register in a state nobody asked for. Better to say so and do nothing.
        var alreadyRegistered = await session
            .Query<WorkerProjection>()
            .CountAsync(w => w.OrganizationId == organization.OrganizationId);

        if (alreadyRegistered > 0)
        {
            logger.LogWarning(
                "Skipping delivery seed for {Slug}: it already has {Count} workers. Clear them first.",
                organization.Slug,
                alreadyRegistered
            );

            return;
        }

        // The workspace's named clients go to the front of the list, so the staffed projects below
        // land on them and the salesperson's screen shows people on a delivery.
        var workspace = new SalesWorkspaceScenario(bus, session, sales);
        var workspaceCompanies = await workspace.CreateCompanies(organization.OrganizationId, userIds[0]);
        companyIds = [.. workspaceCompanies, .. companyIds];

        var legalEntities = await ExistingLegalEntities(organization.OrganizationId);

        if (legalEntities.Count == 0)
        {
            legalEntities = await new LegalEntityScenario(bus).Create(
                organization.OrganizationId,
                userIds[0],
                today
            );

            logger.LogInformation("Created {Count} legal entities", legalEntities.Count);
        }

        // A project reads its delivering entity and its client through snapshot repositories, both
        // of which go to a projection first.
        await WaitForProjections();

        var projects = await new ProjectScenario(bus, () => WaitForProjections()).Create(
            organization.OrganizationId,
            companyIds,
            legalEntities,
            userIds,
            teamIds.Count > 0 ? teamIds[0] : null,
            today
        );

        logger.LogInformation("Created {Count} projects", projects.Count);

        await WaitForProjections();

        var workerCount = await new WorkersScenario(bus, () => WaitForProjections()).Seed(
            organization.OrganizationId,
            projects,
            userIds,
            today
        );

        // Forms are started for workers, and the worker lookup behind a response replays the
        // stream - but picking which workers to use reads the register.
        await WaitForProjections();

        var workerIds = await session
            .Query<WorkerProjection>()
            .Where(w => w.OrganizationId == organization.OrganizationId)
            .OrderBy(w => w.LastName)
            .Select(w => w.Id)
            .ToListAsync();

        var formResponses = await new FormsScenario(bus, session).Seed(
            organization.OrganizationId,
            userIds[0],
            workerIds,
            today
        );

        logger.LogInformation("Seeded forms with {Count} responses", formResponses);

        var tasks = await workspace.Seed(
            organization.OrganizationId,
            workspaceCompanies,
            legalEntities[0].LegalEntityId,
            () => WaitForProjections()
        );

        logger.LogInformation("Seeded the sales workspace with {Count} tasks per person", tasks);

        logger.LogInformation(
            "Delivery seed completed for {Slug}: {WorkerCount} workers across {ProjectCount} projects",
            organization.Slug,
            workerCount,
            projects.Count
        );
    }

    /// <summary>
    /// Our own companies are reserved on their tax id, so a second run has to reuse the ones that
    /// are already there rather than try to create them again.
    /// </summary>
    private async Task<IReadOnlyList<LegalEntityScenario.LegalEntityData>> ExistingLegalEntities(
        Guid organizationId
    )
    {
        var entities = await session
            .Query<LegalEntityProjection>()
            .Where(e => e.OrganizationId == organizationId)
            .ToListAsync();

        return [.. entities.Select(e => new LegalEntityScenario.LegalEntityData(e.Id, e.Name))];
    }
}
