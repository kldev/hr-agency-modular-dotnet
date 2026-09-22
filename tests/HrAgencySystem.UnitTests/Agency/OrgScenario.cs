using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// Builds a chart by replaying the events that would have drawn it. Nothing reaches into the
/// aggregate directly - a test that set up a shape the commands cannot produce would be testing a
/// company that cannot exist.
/// </summary>
internal static class OrgScenario
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid Ceo = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid HeadOfPayroll = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PayrollClerk = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid PayrollSpecialist = Guid.Parse(
        "77777777-7777-7777-7777-777777777777"
    );
    public static readonly Guid OperationsHead = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid PosterAbroad = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static readonly Guid BoardId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid PayrollId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    public static readonly Guid PayrollPolandId = Guid.Parse(
        "aaaaaaaa-0000-0000-0000-000000000003"
    );
    public static readonly Guid OperationsId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");
    public static readonly Guid OperationsAbroadId = Guid.Parse(
        "aaaaaaaa-0000-0000-0000-000000000005"
    );

    public static UserSnapshot User { get; } =
        new(Ceo, "Marek", "Zielinski", "marek.zielinski@hr-agency.com");

    public static IAgencyService Service()
    {
        var service = Substitute.For<IAgencyService>();

        service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(User);
        service
            .GetOrganizationMemberAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo => new UserSnapshot(
                callInfo.ArgAt<Guid>(1),
                "Some",
                "Body",
                "some.body@hr-agency.com"
            ));

        return service;
    }

    /// <summary>
    /// The shape the whole module exists for, in miniature:
    /// <code>
    /// Board (head: CEO)
    /// ├── Payroll (head: head of payroll, plus one specialist)
    /// │   └── Payroll Poland (head: none, so payroll answers for it)
    /// └── Operations (head: operations head)
    ///     └── Operations abroad (head: none)
    /// </code>
    /// </summary>
    public static OrgStructure Company()
    {
        var structure = OrgStructure.Empty();

        structure.Apply(Created(BoardId, null, "Board", OrgUnitKind.Board));
        structure.Apply(MemberAdded(BoardId, Ceo));
        structure.Apply(HeadAssigned(BoardId, Ceo));

        structure.Apply(Created(PayrollId, BoardId, "Payroll", OrgUnitKind.Department));
        structure.Apply(MemberAdded(PayrollId, HeadOfPayroll));
        structure.Apply(HeadAssigned(PayrollId, HeadOfPayroll));
        structure.Apply(MemberAdded(PayrollId, PayrollSpecialist));

        structure.Apply(Created(PayrollPolandId, PayrollId, "Payroll Poland", OrgUnitKind.Section));
        structure.Apply(MemberAdded(PayrollPolandId, PayrollClerk));

        structure.Apply(Created(OperationsId, BoardId, "Operations", OrgUnitKind.Department));
        structure.Apply(MemberAdded(OperationsId, OperationsHead));
        structure.Apply(HeadAssigned(OperationsId, OperationsHead));

        structure.Apply(
            Created(OperationsAbroadId, OperationsId, "Operations abroad", OrgUnitKind.Section)
        );
        structure.Apply(MemberAdded(OperationsAbroadId, PosterAbroad));

        return structure;
    }

    /// <summary>
    /// A board and one empty department with a section under it. Exists so the "still has units
    /// under it" rule can be tested on its own - in <see cref="Company"/> every department also
    /// has people, and the members check fires first.
    /// </summary>
    public static OrgStructure WithEmptyDepartment()
    {
        var structure = OrgStructure.Empty();

        structure.Apply(Created(BoardId, null, "Board", OrgUnitKind.Board));
        structure.Apply(MemberAdded(BoardId, Ceo));
        structure.Apply(HeadAssigned(BoardId, Ceo));

        structure.Apply(Created(PayrollId, BoardId, "Payroll", OrgUnitKind.Department));
        structure.Apply(Created(PayrollPolandId, PayrollId, "Payroll Poland", OrgUnitKind.Section));

        return structure;
    }

    public static OrgUnitCreated Created(
        Guid unitId,
        Guid? parentId,
        string name,
        OrgUnitKind kind
    ) => new(OrganizationId, unitId, parentId, name, kind, User, DateTimeOffset.UtcNow);

    public static OrgUnitMemberAdded MemberAdded(Guid unitId, Guid userId, string title = "") =>
        new(OrganizationId, unitId, new OrgUnitMember(userId, title), User, DateTimeOffset.UtcNow);

    public static OrgUnitHeadAssigned HeadAssigned(Guid unitId, Guid userId) =>
        new(OrganizationId, unitId, userId, User, DateTimeOffset.UtcNow);

    /// <summary>
    /// The same chart, behind the port the handlers ask. Shared, because "who is above whom" is one
    /// fixture and two copies of it would drift.
    /// </summary>
    public static IOrgStructureQueryRepository Chart()
    {
        var structure = Company();

        var projection = new OrgStructureProjection(
            OrgStructureId.For(OrganizationId),
            OrganizationId,
            [
                .. structure.Units.Select(unit => new OrgUnitRow(
                    unit.UnitId,
                    unit.ParentId,
                    unit.Name,
                    unit.Kind,
                    unit.HeadUserId,
                    unit.Members,
                    unit.IsArchived
                )),
            ],
            null,
            null
        );

        var chart = Substitute.For<IOrgStructureQueryRepository>();

        chart
            .GetStructureAsync(Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>())
            .Returns(projection);

        return chart;
    }
}
