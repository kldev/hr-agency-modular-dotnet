using HrAgencySystem.Agency.Application.OrgUnits.AddMember;
using HrAgencySystem.Agency.Application.OrgUnits.Archive;
using HrAgencySystem.Agency.Application.OrgUnits.AssignHead;
using HrAgencySystem.Agency.Application.OrgUnits.Move;
using HrAgencySystem.Agency.Application.OrgUnits.RemoveMember;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The rules that need more than one box to check, which is exactly why the whole chart is one
/// aggregate: split per unit, every one of these would be a question asked of a read model.
/// </summary>
public class OrgUnitHandlerTests : BaseTest
{
    [Fact]
    public async Task Move_UnderItsOwnSection_Refuses()
    {
        // The one shape a tree of parent pointers cannot survive: a ring with no top, which would
        // make the supervisor walk run forever rather than answer.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            MoveOrgUnitHandler.Handle(
                new MoveOrgUnit(
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollId,
                    OrgScenario.PayrollPolandId,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(MoveOrgUnitHandler.IntoOwnSubtreeMessage, error.Message);
    }

    [Fact]
    public async Task Move_UnderItself_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            MoveOrgUnitHandler.Handle(
                new MoveOrgUnit(
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollId,
                    OrgScenario.PayrollId,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(MoveOrgUnitHandler.IntoItselfMessage, error.Message);
    }

    [Fact]
    public async Task Move_TheTopUnit_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            MoveOrgUnitHandler.Handle(
                new MoveOrgUnit(
                    OrgScenario.OrganizationId,
                    OrgScenario.BoardId,
                    OrgScenario.PayrollId,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(MoveOrgUnitHandler.RootCannotMoveMessage, error.Message);
    }

    /// <summary>
    /// A reorganisation moves a box, and everyone inside it gets a new supervisor without anybody
    /// editing a person - which is the whole reason the supervisor is computed.
    /// </summary>
    [Fact]
    public async Task Move_ReparentsTheUnitAndWithItEverybodyInside()
    {
        var structure = OrgScenario.Company();

        var (moved, _) = await MoveOrgUnitHandler.Handle(
            new MoveOrgUnit(
                OrgScenario.OrganizationId,
                OrgScenario.PayrollPolandId,
                OrgScenario.OperationsId,
                OrgScenario.Ceo
            ),
            structure,
            OrgScenario.Service(),
            Clock(),
            CancellationToken.None
        );

        structure.Apply(moved);

        Assert.Equal(
            OrgScenario.OperationsHead,
            SupervisorPolicy.SupervisorOf(structure.Units, OrgScenario.PayrollClerk)
        );
    }

    [Fact]
    public async Task AddMember_SomebodyAlreadyInAnotherUnit_Refuses()
    {
        // One person, one unit. Somebody working for two departments still has one supervisor, and
        // that is the right answer rather than a gap to be filled.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            AddOrgUnitMemberHandler.Handle(
                new AddOrgUnitMember(
                    OrgScenario.OrganizationId,
                    OrgScenario.OperationsId,
                    OrgScenario.PayrollClerk,
                    null,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(AddOrgUnitMemberHandler.AlreadyInAnotherUnitMessage, error.Message);
    }

    [Fact]
    public async Task AssignHead_SomebodyWhoIsNotInTheUnit_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            AssignOrgUnitHeadHandler.Handle(
                new AssignOrgUnitHead(
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollPolandId,
                    OrgScenario.OperationsHead,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(AssignOrgUnitHeadHandler.NotAMemberMessage, error.Message);
    }

    [Fact]
    public async Task RemoveMember_TheHead_Refuses()
    {
        // Taking the head out silently would leave a unit headed by somebody who is not in it, and
        // the supervisor walk would keep answering with them.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            RemoveOrgUnitMemberHandler.Handle(
                new RemoveOrgUnitMember(
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollId,
                    OrgScenario.HeadOfPayroll,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(RemoveOrgUnitMemberHandler.IsTheHeadMessage, error.Message);
    }

    [Fact]
    public async Task Archive_AUnitThatStillHasPeople_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ArchiveOrgUnitHandler.Handle(
                new ArchiveOrgUnit(
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollPolandId,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(ArchiveOrgUnitHandler.StillHasPeopleMessage, error.Message);
    }

    [Fact]
    public async Task Archive_AUnitThatStillHasUnitsUnderIt_Refuses()
    {
        // Emptied of people but not of sections: archiving it would hide a whole branch behind a
        // box nobody lists any more.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ArchiveOrgUnitHandler.Handle(
                new ArchiveOrgUnit(
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollId,
                    OrgScenario.Ceo
                ),
                OrgScenario.WithEmptyDepartment(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(ArchiveOrgUnitHandler.StillHasUnitsMessage, error.Message);
    }

    [Fact]
    public async Task Archive_TheTopUnit_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ArchiveOrgUnitHandler.Handle(
                new ArchiveOrgUnit(
                    OrgScenario.OrganizationId,
                    OrgScenario.BoardId,
                    OrgScenario.Ceo
                ),
                OrgScenario.Company(),
                OrgScenario.Service(),
                Clock(),
                CancellationToken.None
            )
        );

        Assert.Equal(ArchiveOrgUnitHandler.RootCannotBeArchivedMessage, error.Message);
    }

    private static FixedClock Clock() => new(DateTimeOffset.UtcNow);
}
