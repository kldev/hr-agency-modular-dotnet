using Bogus;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal class UserScenario(IMessageBus bus)
{
    private static readonly OrganizationRole[] SeedRoles =
        Enum.GetValues<OrganizationRole>()
            .Where(role => role != OrganizationRole.System)
            .ToArray();

    internal async Task<IReadOnlyList<Guid>> Create(
        OrganizationScenario.OrganizationData data,
        int seedCount = 10)
    {
        if (seedCount < 2)
            throw new ArgumentOutOfRangeException(
                nameof(seedCount),
                "Seed count must be at least 2.");

        var domain = $"@{data.slug}.com";
        const string userPassword = "test123";

        var users = new List<CreateUser>
        {
            new(
                data.OrganizationId,
                $"j.smith{domain}",
                "John",
                "Smith",
                OrganizationRole.Admin,
                userPassword, Guid.Empty),

            new(
                data.OrganizationId,
                $"kate.rec{domain}",
                "Katy",
                "Wells",
                OrganizationRole.Recruiter,
                userPassword, Guid.Empty)
        };

        var faker = new Faker();

        for (var i = users.Count; i < seedCount; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var role = faker.PickRandom(SeedRoles);

            users.Add(new CreateUser(
                data.OrganizationId,
                $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}{i}{domain}",
                firstName,
                lastName,
                role,
                userPassword, Guid.Empty));
        }

        var ids = new List<Guid>();
        
        foreach (var user in users)
        {
            var result = await bus.InvokeAsync<UserCreated>(user);
            ids.Add(result.UserId);
        }

        return ids;
    }
}