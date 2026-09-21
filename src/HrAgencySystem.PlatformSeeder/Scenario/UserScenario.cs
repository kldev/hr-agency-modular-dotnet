using Bogus;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal class UserScenario(IMessageBus bus)
{
    private static readonly OrganizationRole[] SeedRoles = Enum.GetValues<OrganizationRole>()
        .Where(role => role != OrganizationRole.System)
        .ToArray();

    internal async Task<IReadOnlyList<Guid>> Create(
        OrganizationScenario.OrganizationData data,
        int seedCount = 10
    )
    {
        if (seedCount < 2)
            throw new ArgumentOutOfRangeException(
                nameof(seedCount),
                "Seed count must be at least 2."
            );

        var domain = $"@{data.Slug}.com";
        const string userPassword = Config.TestPassword;

        var faker = new Faker();

        var users = new List<CreateUser>
        {
            new(
                data.OrganizationId,
                new ContactPerson(
                    $"j.smith{domain}",
                    "John",
                    "Smith",
                    JobTitleFor(OrganizationRole.Admin),
                    faker.Phone.PhoneNumber()
                ),
                OrganizationRole.Admin,
                userPassword,
                Guid.Empty
            ),
            new(
                data.OrganizationId,
                new ContactPerson(
                    $"kate.rec{domain}",
                    "Katy",
                    "Wells",
                    JobTitleFor(OrganizationRole.Recruiter),
                    faker.Phone.PhoneNumber()
                ),
                OrganizationRole.Recruiter,
                userPassword,
                Guid.Empty
            ),
            new(
                data.OrganizationId,
                new ContactPerson(
                    $"bob.sale{domain}",
                    "Bob",
                    "Wells",
                    JobTitleFor(OrganizationRole.Sales),
                    faker.Phone.PhoneNumber()
                ),
                OrganizationRole.Sales,
                userPassword,
                Guid.Empty
            ),
            new(
                data.OrganizationId,
                new ContactPerson(
                    $"adrian.sal{domain}",
                    "Adrian",
                    "Jimbo",
                    JobTitleFor(OrganizationRole.Sales),
                    faker.Phone.PhoneNumber()
                ),
                OrganizationRole.Sales,
                userPassword,
                Guid.Empty
            ),
        };

        for (var i = users.Count; i < seedCount; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var role = faker.PickRandom(SeedRoles);

            users.Add(
                new CreateUser(
                    data.OrganizationId,
                    new ContactPerson(
                        $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}{i}{Random.Shared.Next(1000, 99999)}{domain}",
                        firstName,
                        lastName,
                        JobTitleFor(role),
                        faker.Phone.PhoneNumber()
                    ),
                    role,
                    userPassword,
                    Guid.Empty
                )
            );
        }

        var ids = new List<Guid>();

        foreach (var user in users)
        {
            var result = await bus.InvokeAsync<UserCreated>(user);
            ids.Add(result.UserId);
        }

        return ids;
    }

    private static string JobTitleFor(OrganizationRole role) =>
        role switch
        {
            OrganizationRole.Admin => "Administrator",
            OrganizationRole.Recruiter => "Recruiter",
            OrganizationRole.HiringManager => "Hiring Manager",
            OrganizationRole.Interviewer => "Interviewer",
            OrganizationRole.Sales => "Sales Specialist",
            OrganizationRole.HumanResources => "HR Specialist",
            OrganizationRole.Finance => "Finance Specialist",
            OrganizationRole.Administration => "Office Administrator",
            _ => "Employee",
        };
}
