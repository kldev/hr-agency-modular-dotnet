using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// "Is this person above that one" - the question every approval asks. Separate from
/// <see cref="SupervisorPolicyTests"/> because that one asks for a single answer and this one asks
/// about reach, which is what "their own and everybody under them" means.
/// </summary>
public class SupervisorReachTests : BaseTest
{
    [Fact]
    public void TheHeadOfMyUnitIsAboveMe()
    {
        var units = OrgScenario.Company().Units;

        Assert.True(
            SupervisorPolicy.IsAbove(
                units,
                OrgScenario.HeadOfPayroll,
                OrgScenario.PayrollSpecialist
            )
        );
    }

    /// <summary>Reach runs the whole way up, not one floor.</summary>
    [Fact]
    public void SoIsAnybodyFurtherUpTheSameLine()
    {
        var units = OrgScenario.Company().Units;

        Assert.True(
            SupervisorPolicy.IsAbove(units, OrgScenario.Ceo, OrgScenario.PayrollSpecialist)
        );
        Assert.True(SupervisorPolicy.IsAbove(units, OrgScenario.Ceo, OrgScenario.PosterAbroad));
    }

    /// <summary>
    /// A section with no head of its own is answered for from above - the same rule that makes the
    /// supervisor lookup work makes the reach check work.
    /// </summary>
    [Fact]
    public void AHeadReachesIntoASectionWithNoHeadOfItsOwn()
    {
        var units = OrgScenario.Company().Units;

        Assert.True(
            SupervisorPolicy.IsAbove(units, OrgScenario.HeadOfPayroll, OrgScenario.PayrollClerk)
        );
    }

    /// <summary>Sideways is not up. This is the case a role-based check would let through.</summary>
    [Fact]
    public void AHeadOfAnotherBranchIsNotAboveMe()
    {
        var units = OrgScenario.Company().Units;

        Assert.False(
            SupervisorPolicy.IsAbove(units, OrgScenario.OperationsHead, OrgScenario.PayrollClerk)
        );

        Assert.False(
            SupervisorPolicy.IsAbove(units, OrgScenario.HeadOfPayroll, OrgScenario.PosterAbroad)
        );
    }

    [Fact]
    public void NobodyIsAboveThemselves()
    {
        var units = OrgScenario.Company().Units;

        Assert.False(
            SupervisorPolicy.IsAbove(units, OrgScenario.HeadOfPayroll, OrgScenario.HeadOfPayroll)
        );

        Assert.False(SupervisorPolicy.IsAbove(units, OrgScenario.Ceo, OrgScenario.Ceo));
    }

    /// <summary>Somebody who sits in no unit has nobody above them, so nobody can decide for them.</summary>
    [Fact]
    public void SomebodyOutsideTheChartIsUnreachable()
    {
        var units = OrgScenario.Company().Units;

        Assert.False(SupervisorPolicy.IsAbove(units, OrgScenario.Ceo, Guid.NewGuid()));
    }
}
