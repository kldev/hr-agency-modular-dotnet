using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The rule the whole module exists to serve. Pure, so it is tested here rather than through a
/// database - and it is worth testing hard, because leave and timesheets will both hang off it.
/// </summary>
public class SupervisorPolicyTests : BaseTest
{
    [Fact]
    public void SupervisorOf_IsTheHeadOfTheUnitSomebodySitsIn()
    {
        var units = OrgScenario.Company().Units;

        Assert.Equal(
            OrgScenario.HeadOfPayroll,
            SupervisorPolicy.SupervisorOf(units, OrgScenario.PayrollSpecialist)
        );
    }

    /// <summary>
    /// The case the walking-up rule exists for. "Payroll Poland" has no head of its own, so the
    /// head of payroll answers for the people in it - and nothing in that section says so.
    /// </summary>
    [Fact]
    public void SupervisorOf_AUnitWithoutItsOwnHead_IsAnsweredForFromAbove()
    {
        var units = OrgScenario.Company().Units;

        Assert.Equal(
            OrgScenario.HeadOfPayroll,
            SupervisorPolicy.SupervisorOf(units, OrgScenario.PayrollClerk)
        );

        Assert.Equal(
            OrgScenario.OperationsHead,
            SupervisorPolicy.SupervisorOf(units, OrgScenario.PosterAbroad)
        );
    }

    /// <summary>The other half of the same sentence: a head reports a floor up, not to themselves.</summary>
    [Fact]
    public void SupervisorOf_AHead_IsTheHeadAbove()
    {
        var units = OrgScenario.Company().Units;

        Assert.Equal(
            OrgScenario.Ceo,
            SupervisorPolicy.SupervisorOf(units, OrgScenario.HeadOfPayroll)
        );
    }

    [Fact]
    public void SupervisorOf_TheTop_IsNobody()
    {
        var units = OrgScenario.Company().Units;

        // Not a gap: there is nobody above the chief executive, and leave will have to decide what
        // that means rather than wait for somebody to fill a field in.
        Assert.Null(SupervisorPolicy.SupervisorOf(units, OrgScenario.Ceo));
    }

    [Fact]
    public void SupervisorOf_SomebodyOutsideTheChart_IsNobody()
    {
        var units = OrgScenario.Company().Units;

        Assert.Null(SupervisorPolicy.SupervisorOf(units, Guid.NewGuid()));
    }

    [Fact]
    public void SubordinatesOf_AHead_IsTheirOwnUnitOnly()
    {
        var units = OrgScenario.Company().Units;

        var direct = SupervisorPolicy.SubordinatesOf(
            units,
            OrgScenario.HeadOfPayroll,
            wholeSubtree: false
        );

        // Their own people, themselves excluded - nobody is their own subordinate. The section
        // below is not theirs until the whole subtree is asked for.
        Assert.Equal([OrgScenario.PayrollSpecialist], direct);
    }

    [Fact]
    public void SubordinatesOf_WholeSubtree_ReachesTheSectionsBelow()
    {
        var units = OrgScenario.Company().Units;

        var all = SupervisorPolicy.SubordinatesOf(
            units,
            OrgScenario.HeadOfPayroll,
            wholeSubtree: true
        );

        Assert.Contains(OrgScenario.PayrollClerk, all);
        Assert.Contains(OrgScenario.PayrollSpecialist, all);
        Assert.DoesNotContain(OrgScenario.HeadOfPayroll, all);
    }

    [Fact]
    public void SubordinatesOf_TheChiefExecutive_ReachesEverybody()
    {
        var units = OrgScenario.Company().Units;

        var all = SupervisorPolicy.SubordinatesOf(units, OrgScenario.Ceo, wholeSubtree: true);

        Assert.Contains(OrgScenario.HeadOfPayroll, all);
        Assert.Contains(OrgScenario.PayrollClerk, all);
        Assert.Contains(OrgScenario.OperationsHead, all);
        Assert.Contains(OrgScenario.PosterAbroad, all);
    }

    [Fact]
    public void Descendants_AreWhatTheMoveRuleRefusesToHangAUnitUnder()
    {
        var structure = OrgScenario.Company();
        var payroll = structure.UnitById(OrgScenario.PayrollId)!;

        var below = SupervisorPolicy.Descendants(structure.Units, payroll);

        Assert.Contains(below, unit => unit.UnitId == OrgScenario.PayrollPolandId);
        Assert.DoesNotContain(below, unit => unit.UnitId == OrgScenario.OperationsId);
    }
}
