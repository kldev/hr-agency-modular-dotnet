using HrAgencySystem.Agency.Application.OrgUnits.AddMember;
using HrAgencySystem.Agency.Application.OrgUnits.AssignHead;
using HrAgencySystem.Agency.Application.OrgUnits.Create;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// The agency's own chart, with a named person at the head of every box.
/// <para>
/// The heads are seeded here rather than taken from the general pool, and that is deliberate: a
/// head is the one member of a unit whose name has to match the unit, and a chart whose boxes are
/// headed by whoever happened to be next in a list teaches nothing. The general staff are then
/// seated underneath, so the register shows what a real one looks like - departments with several
/// people, not one each.
/// </para>
/// <para>
/// Two shapes are here on purpose rather than for variety: payroll and operations have sections
/// under them, and the operations sections get no head of their own. That second one is the case
/// the whole walking-up rule exists for - the people there answer to the head of operations, and
/// nothing in their own unit says so.
/// </para>
/// </summary>
internal sealed class OrgStructureScenario(IMessageBus bus)
{
    private sealed record UnitSpec(
        string Name,
        OrgUnitKind Kind,
        string HeadFirstName,
        string HeadLastName,
        string HeadTitle,
        OrganizationRole HeadRole,
        bool WantsHead = true,
        UnitSpec[]? Children = null
    );

    private static readonly UnitSpec BoardSpec = new(
        "Board",
        OrgUnitKind.Board,
        "Marek",
        "Zielinski",
        "Chief Executive Officer",
        OrganizationRole.Admin
    );

    /// <summary>The second owner: on the board, with a title of their own, not heading it.</summary>
    private static readonly (string First, string Last, string Title) CoOwner = (
        "Pawel",
        "Zielinski",
        "Co-owner"
    );

    private static readonly UnitSpec[] Departments =
    [
        new("IT", OrgUnitKind.Department, "Tomasz", "Lis", "Head of IT", OrganizationRole.Admin),
        new(
            "Finance",
            OrgUnitKind.Department,
            "Joanna",
            "Krol",
            "Head of Finance",
            OrganizationRole.Admin
        ),
        new(
            "Administration",
            OrgUnitKind.Department,
            "Beata",
            "Sikora",
            "Head of Administration",
            OrganizationRole.Admin
        ),
        new(
            "Recruitment",
            OrgUnitKind.Department,
            "Agnieszka",
            "Mazur",
            "Head of Recruitment",
            OrganizationRole.Recruiter
        ),
        new(
            "Legalisation",
            OrgUnitKind.Department,
            "Iryna",
            "Kovalenko",
            "Head of Legalisation",
            OrganizationRole.Admin
        ),
        new(
            "Payroll",
            OrgUnitKind.Department,
            "Halina",
            "Dabrowska",
            "Head of Payroll",
            OrganizationRole.Admin,
            Children:
            [
                new(
                    "Payroll Poland",
                    OrgUnitKind.Section,
                    "Ewa",
                    "Nowicka",
                    "Payroll Poland lead",
                    OrganizationRole.Admin
                ),
                new(
                    "Payroll abroad",
                    OrgUnitKind.Section,
                    "Monika",
                    "Bak",
                    "Payroll abroad lead",
                    OrganizationRole.Admin
                ),
            ]
        ),
        new(
            "Operations",
            OrgUnitKind.Department,
            "Rafal",
            "Wojcik",
            "Head of Operations",
            OrganizationRole.HiringManager,
            Children:
            [
                // No heads of their own: the head of operations answers for both, and the
                // supervisor rule finds him by walking up rather than by anybody writing it down.
                new(
                    "Operations Poland",
                    OrgUnitKind.Section,
                    "",
                    "",
                    "",
                    OrganizationRole.HiringManager,
                    WantsHead: false
                ),
                new(
                    "Operations abroad",
                    OrgUnitKind.Section,
                    "",
                    "",
                    "",
                    OrganizationRole.HiringManager,
                    WantsHead: false
                ),
            ]
        ),
        new(
            "Sales",
            OrgUnitKind.Department,
            "Krzysztof",
            "Baran",
            "Head of Sales",
            OrganizationRole.Sales
        ),
    ];

    private int _created;

    internal async Task<int> Create(
        Guid organizationId,
        string slug,
        IReadOnlyList<Guid> staffIds,
        Func<Task> waitForProjections
    )
    {
        _created = 0;

        // The chief executive exists before the chart does, because creating a unit records who
        // created it - there is no bootstrapping trick here, just an order that works.
        var ceo = await CreateUser(
            organizationId,
            slug,
            BoardSpec.HeadFirstName,
            BoardSpec.HeadLastName,
            BoardSpec.HeadTitle,
            BoardSpec.HeadRole
        );

        var board = await CreateUnit(organizationId, null, BoardSpec, ceo);

        await AddMember(organizationId, board, ceo, ceo, BoardSpec.HeadTitle);
        await AssignHead(organizationId, board, ceo, ceo);

        var coOwner = await CreateUser(
            organizationId,
            slug,
            CoOwner.First,
            CoOwner.Last,
            CoOwner.Title,
            OrganizationRole.Admin
        );

        await AddMember(organizationId, board, coOwner, ceo, CoOwner.Title);

        await waitForProjections();

        // Where the general staff go. The board is left out: an agency does not seat its recruiters
        // on the board, and a chart that did would answer the supervisor question wrongly.
        var staffable = new List<Guid>();

        foreach (var department in Departments)
        {
            var unitId = await CreateUnit(organizationId, board, department, ceo);
            await SeatHead(organizationId, slug, unitId, department, ceo);

            if (department.Children is null)
                staffable.Add(unitId);

            foreach (var section in department.Children ?? [])
            {
                var sectionId = await CreateUnit(organizationId, unitId, section, ceo);
                await SeatHead(organizationId, slug, sectionId, section, ceo);

                staffable.Add(sectionId);
            }
        }

        await Distribute(organizationId, staffable, staffIds, ceo);

        return _created;
    }

    /// <summary>
    /// Seats the rest of the agency round the chart. One person belongs to one unit, so the pool is
    /// walked once - somebody already placed as a head is never handed out again.
    /// </summary>
    private async Task Distribute(
        Guid organizationId,
        IReadOnlyList<Guid> units,
        IReadOnlyList<Guid> staffIds,
        Guid modifiedBy
    )
    {
        if (units.Count == 0)
            return;

        var index = 0;

        foreach (var userId in staffIds)
        {
            await AddMember(organizationId, units[index % units.Count], userId, modifiedBy, null);
            index++;
        }
    }

    private async Task<Guid> CreateUnit(
        Guid organizationId,
        Guid? parentId,
        UnitSpec spec,
        Guid createdBy
    )
    {
        var result = await bus.InvokeAsync<OrgUnitCreated>(
            new CreateOrgUnit(organizationId, parentId, spec.Name, spec.Kind, createdBy)
        );

        _created++;

        return result.UnitId;
    }

    /// <summary>
    /// Creates the named head, puts them in the unit and then names them - in that order, because
    /// the head of a unit is one of its own people and the command refuses anything else.
    /// </summary>
    private async Task<Guid> SeatHead(
        Guid organizationId,
        string slug,
        Guid unitId,
        UnitSpec spec,
        Guid modifiedBy
    )
    {
        if (!spec.WantsHead)
            return Guid.Empty;

        var headId = await CreateUser(
            organizationId,
            slug,
            spec.HeadFirstName,
            spec.HeadLastName,
            spec.HeadTitle,
            spec.HeadRole
        );

        await AddMember(organizationId, unitId, headId, modifiedBy, spec.HeadTitle);
        await AssignHead(organizationId, unitId, headId, modifiedBy);

        return headId;
    }

    private async Task AssignHead(
        Guid organizationId,
        Guid unitId,
        Guid headUserId,
        Guid modifiedBy
    ) =>
        await bus.InvokeAsync<OrgUnitHeadAssigned>(
            new AssignOrgUnitHead(organizationId, unitId, headUserId, modifiedBy)
        );

    private async Task<Guid> CreateUser(
        Guid organizationId,
        string slug,
        string firstName,
        string lastName,
        string title,
        OrganizationRole role
    )
    {
        var email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@{slug}.com";

        var result = await bus.InvokeAsync<UserCreated>(
            new CreateUser(
                organizationId,
                new ContactPerson(email, firstName, lastName, title, "+48 600 100 200"),
                role,
                Config.TestPassword,
                Guid.Empty
            )
        );

        return result.UserId;
    }

    private async Task AddMember(
        Guid organizationId,
        Guid unitId,
        Guid userId,
        Guid modifiedBy,
        string? title
    ) =>
        await bus.InvokeAsync<OrgUnitMemberAdded>(
            new AddOrgUnitMember(organizationId, unitId, userId, title, modifiedBy)
        );
}
