using HrAgencySystem.Company.Events;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.PlatformSeeder.Scenario;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder;

public sealed class HrAgencyShowcaseSeeder(
    IMessageBus bus,
    IQuerySession session) : IPlatformSeeder
{
    private const int ProjectionDelayMs = 5_000;

    public async Task Seed()
    {
        var owner = await new OwnerScenario(bus).Create();

        await SeedAgency(owner.PlatformOwnerId, new SeedConfig());

        await SeedAgency(
            owner.PlatformOwnerId,
            new SeedConfig(
                Name: "Flex Jobs",
                Slug: "flex-jobs",
                UsersCount: 50,
                CompaniesCount: 999));
    }

    public async Task SeedApplicants(int count)
    {
        var organization = await session.Query<OrganizationCreated>().ToListAsync();
        foreach (var org in organization)
        {
            await GenerateApplicants(count);
        }
    }

    public async Task SeedShowcase()
    {
        var organization = await session.Query<OrganizationCreated>().ToListAsync();
        foreach (var org in organization)
        {
            var userIds = await session.Query<UserCreated>().Where(z => z.OrganizationId == org.OrganizationId).Select(z=>z.UserId)
                .ToListAsync();
            var companyIds = await session.Query<CompanyCreated>().Where(z => z.OrganizationId == org.OrganizationId).Select(z=>z.CompanyId)
                .ToListAsync();

            await new ModernWebDeveloperScenario(bus).Create(org.OrganizationId, userIds, companyIds);
        }

        
        await new ApplyToJobPostScenario(bus, session).ExecuteShowcase();
    }

    private async Task SeedAgency(Guid ownerId, SeedConfig config)
    {
        var organization = await new OrganizationScenario(bus)
            .Create(ownerId, config.Name, config.Slug);

        var userIds = await CreateUsers(config, organization);

        await WaitForProjections();

        var companyIds = await CreateCompanies(config, organization, userIds);

        await WaitForProjections();

        await CreateProductionJobDescription(organization, userIds, companyIds);

        var javaJobDescriptionId = 
            await CreateTechnicalJobsDescription(organization, userIds, companyIds);

        await CreateMoreJobPosting(javaJobDescriptionId, organization, userIds);

        await CreateModernDeveloperPosts(organization, userIds, companyIds);

        await WaitForProjections();

        await PostToChannel(userIds);

        await GenerateApplicants(20);

    }

    #region Tasks
    
    private async Task PostToChannel(IReadOnlyList<Guid> userIds)
    {
        await new PostJobToRandomChannelScenario(bus, session)
            .Execute(userIds);
    }
    
    private async Task GenerateApplicants(int count = 500)
    {
        await new ApplyToJobPostScenario(bus, session)
            .Execute(count);
    }
    

    private async Task CreateModernDeveloperPosts(OrganizationScenario.OrganizationData organization, IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        await new ModernWebDeveloperScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                companyIds);
    }

    private async Task CreateMoreJobPosting(Guid javaJobDescriptionId, OrganizationScenario.OrganizationData organization, IReadOnlyList<Guid> userIds)
    {
        await new JobPostScenario(bus)
            .Create(
                javaJobDescriptionId,
                organization.OrganizationId,
                userIds.First());
    }

    private async Task<Guid> CreateTechnicalJobsDescription(OrganizationScenario.OrganizationData organization, IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        var javaJobDescriptionId =
            await new TechnicalJobDescriptionScenario(bus)
                .Create(
                    organization.OrganizationId,
                    userIds,
                    companyIds);
        return javaJobDescriptionId;
    }

    private async Task CreateProductionJobDescription(OrganizationScenario.OrganizationData organization, IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        await new ProductionJobDescriptionScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                companyIds);
    }

    private async Task<IReadOnlyList<Guid>> CreateCompanies(SeedConfig config, OrganizationScenario.OrganizationData organization, IReadOnlyList<Guid> userIds)
    {
        var companyIds = await new CompanyScenario(bus)
            .Create(
                organization.OrganizationId,
                userIds,
                config.CompaniesCount);
        return companyIds;
    }

    private async Task<IReadOnlyList<Guid>> CreateUsers(SeedConfig config, OrganizationScenario.OrganizationData organization)
    {
        var userIds = await new UserScenario(bus)
            .Create(organization, config.UsersCount);
        return userIds;
    }
    
    #endregion

    private static Task WaitForProjections() =>
        Task.Delay(ProjectionDelayMs);

    private sealed record SeedConfig(
        string Name = "Hr Agency",
        string Slug = "hr-agency",
        int UsersCount = 20,
        int CompaniesCount = 101);
}