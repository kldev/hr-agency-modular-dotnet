using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Agency.Application.OrgUnits.AddMember;
using HrAgencySystem.Agency.Application.OrgUnits.Create;
using HrAgencySystem.Agency.Application.OrgUnits.Move;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Agency;

/// <summary>
/// The agency's own chart. Nothing here is visible on its own - it exists so leave and timesheets
/// have somebody to send an approval to.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class OrgStructureTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanOrgStructure();
        await Cleaner.CleanUsers();
    }

    /// <summary>
    /// The case the walking-up rule exists for, end to end: a section with no head of its own, and
    /// the head of the department above answering for the people in it.
    /// </summary>
    [Fact]
    public async Task A_section_without_its_own_head_is_answered_for_from_above()
    {
        var organizationId = Guid.NewGuid();

        var ceo = await NewUserAsync(organizationId, "ceo");
        var headOfOperations = await NewUserAsync(organizationId, "ops");
        var poster = await NewUserAsync(organizationId, "poster");

        var board = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );
        await OrgStructureClient.AddMemberAsync(organizationId, board, ceo);
        await OrgStructureClient.AssignHeadAsync(organizationId, board, ceo);

        var operations = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            board,
            "Operations"
        );
        await OrgStructureClient.AddMemberAsync(organizationId, operations, headOfOperations);
        await OrgStructureClient.AssignHeadAsync(organizationId, operations, headOfOperations);

        var abroad = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            operations,
            "Operations abroad",
            OrgUnitKind.Section
        );
        await OrgStructureClient.AddMemberAsync(organizationId, abroad, poster);

        var supervisor = await OrgStructureClient.GetSupervisorAsync(organizationId, poster);

        Assert.NotNull(supervisor);
        Assert.Equal(headOfOperations, supervisor.UserId);
        Assert.Equal("Operations", supervisor.UnitName);

        // And the head of operations reports a floor up rather than to themselves.
        var above = await OrgStructureClient.GetSupervisorAsync(organizationId, headOfOperations);
        Assert.Equal(ceo, above?.UserId);

        // Nobody is above the chief executive, and that is an answer rather than a gap.
        Assert.Null(await OrgStructureClient.GetSupervisorAsync(organizationId, ceo));
    }

    /// <summary>
    /// A reorganisation is a box changing what it hangs under, and everybody inside gets a new
    /// supervisor without a single person being edited. That is the whole reason it is computed.
    /// </summary>
    [Fact]
    public async Task Moving_a_unit_moves_the_supervisor_of_everybody_in_it()
    {
        var organizationId = Guid.NewGuid();

        var ceo = await NewUserAsync(organizationId, "ceo");
        var headOfPayroll = await NewUserAsync(organizationId, "payroll");
        var headOfOperations = await NewUserAsync(organizationId, "ops");
        var clerk = await NewUserAsync(organizationId, "clerk");

        var board = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );
        await OrgStructureClient.AddMemberAsync(organizationId, board, ceo);
        await OrgStructureClient.AssignHeadAsync(organizationId, board, ceo);

        var payroll = await OrgStructureClient.CreateUnitAsync(organizationId, board, "Payroll");
        await OrgStructureClient.AddMemberAsync(organizationId, payroll, headOfPayroll);
        await OrgStructureClient.AssignHeadAsync(organizationId, payroll, headOfPayroll);

        var operations = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            board,
            "Operations"
        );
        await OrgStructureClient.AddMemberAsync(organizationId, operations, headOfOperations);
        await OrgStructureClient.AssignHeadAsync(organizationId, operations, headOfOperations);

        var section = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            payroll,
            "Payroll Poland",
            OrgUnitKind.Section
        );
        await OrgStructureClient.AddMemberAsync(organizationId, section, clerk);

        var before = await OrgStructureClient.GetSupervisorAsync(organizationId, clerk);
        Assert.Equal(headOfPayroll, before?.UserId);

        var moved = await OrgStructureClient.MoveUnitResponseAsync(
            organizationId,
            section,
            operations
        );
        moved.EnsureSuccessStatusCode();

        var after = await OrgStructureClient.GetSupervisorAsync(organizationId, clerk);
        Assert.Equal(headOfOperations, after?.UserId);
    }

    [Fact]
    public async Task A_unit_cannot_be_moved_under_its_own_section()
    {
        var organizationId = Guid.NewGuid();

        var board = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );
        var payroll = await OrgStructureClient.CreateUnitAsync(organizationId, board, "Payroll");
        var section = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            payroll,
            "Payroll Poland",
            OrgUnitKind.Section
        );

        var response = await OrgStructureClient.MoveUnitResponseAsync(
            organizationId,
            payroll,
            section
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(MoveOrgUnitHandler.IntoOwnSubtreeMessage, problem?.Detail);
    }

    [Fact]
    public async Task There_is_exactly_one_top_unit()
    {
        var organizationId = Guid.NewGuid();

        await OrgStructureClient.CreateUnitAsync(organizationId, null, "Board", OrgUnitKind.Board);

        var response = await OrgStructureClient.CreateUnitResponseAsync(
            organizationId,
            null,
            "Second board"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(CreateOrgUnitHandler.RootAlreadyExistsMessage, problem?.Detail);
    }

    [Fact]
    public async Task Nobody_belongs_to_two_units()
    {
        var organizationId = Guid.NewGuid();
        var userId = await NewUserAsync(organizationId, "one");

        var board = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );
        var payroll = await OrgStructureClient.CreateUnitAsync(organizationId, board, "Payroll");

        await OrgStructureClient.AddMemberAsync(organizationId, board, userId);

        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(
            $"/api/org-structure/units/{payroll}/members",
            new { userId, title = (string?)null }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(AddOrgUnitMemberHandler.AlreadyInAnotherUnitMessage, problem?.Detail);
    }

    /// <summary>The whole chart in one read - there is nothing to page and every question needs it whole.</summary>
    [Fact]
    public async Task The_chart_is_read_whole()
    {
        var organizationId = Guid.NewGuid();

        var board = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );
        await OrgStructureClient.CreateUnitAsync(organizationId, board, "Payroll");
        await OrgStructureClient.CreateUnitAsync(organizationId, board, "Sales");

        await Eventually.AssertAsync(async () =>
        {
            var structure = await OrgStructureClient.GetAsync(organizationId);

            Assert.NotNull(structure);
            Assert.Equal(3, structure.Units.Count);
            Assert.Contains(structure.Units, unit => unit.Name == "Payroll");
        });
    }

    /// <summary>
    /// The chart is opened for an organization that really exists as an aggregate, not for a bare
    /// guid. That difference is not cosmetic: Marten's streams share one table across every module
    /// and the organization aggregate already owns the stream whose id is the organization id, so
    /// a chart sitting on that same id dies with "Stream #… already exists". Every other test here
    /// uses a loose guid and would never have noticed - the seeder did.
    /// </summary>
    [Fact]
    public async Task A_chart_can_be_opened_for_a_real_organization()
    {
        var slug = $"agency-{Guid.NewGuid():N}"[..20];

        // Creating an organization is the platform owner's call, not an agency user's.
        var organization = await new Organization.OrganizationTestClient(
            Env.CreateClient().AsOwner(),
            OutputHelper
        ).CreateAsync($"Agency {slug}", slug);

        var board = await OrgStructureClient.CreateUnitAsync(
            organization.OrganizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );

        var payroll = await OrgStructureClient.CreateUnitAsync(
            organization.OrganizationId,
            board,
            "Payroll"
        );

        Assert.NotEqual(Guid.Empty, payroll);
    }

    private async Task<Guid> NewUserAsync(Guid organizationId, string handle)
    {
        var user = await UserClient.CreateAsync(
            organizationId,
            email: $"{handle}-{Guid.NewGuid():N}@agency.test"
        );

        return user.Id;
    }
}
