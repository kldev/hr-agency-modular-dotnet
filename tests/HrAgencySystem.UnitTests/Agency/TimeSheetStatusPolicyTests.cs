using HrAgencySystem.Agency.Domain.TimeSheets;

namespace HrAgencySystem.UnitTests.Agency;

public class TimeSheetStatusPolicyTests : BaseTest
{
    [Theory]
    [InlineData(TimeSheetStatus.Draft, TimeSheetStatus.Submitted)]
    [InlineData(TimeSheetStatus.Submitted, TimeSheetStatus.Approved)]
    [InlineData(TimeSheetStatus.Submitted, TimeSheetStatus.Correction)]
    [InlineData(TimeSheetStatus.Approved, TimeSheetStatus.Settled)]
    [InlineData(TimeSheetStatus.Approved, TimeSheetStatus.Correction)]
    [InlineData(TimeSheetStatus.Correction, TimeSheetStatus.Submitted)]
    public void TheFlowGoesForwardAndBackToCorrection(TimeSheetStatus from, TimeSheetStatus to) =>
        Assert.True(TimeSheetStatusChangePolicy.CanChange(from, to));

    /// <summary>
    /// A month that has been handed to payroll is the end of the line. Reopening it is a different
    /// feature with a different name, and it waits for somebody to actually ask for it.
    /// </summary>
    [Theory]
    [InlineData(TimeSheetStatus.Settled, TimeSheetStatus.Correction)]
    [InlineData(TimeSheetStatus.Settled, TimeSheetStatus.Submitted)]
    [InlineData(TimeSheetStatus.Settled, TimeSheetStatus.Approved)]
    public void ASettledMonthGoesNowhere(TimeSheetStatus from, TimeSheetStatus to) =>
        Assert.False(TimeSheetStatusChangePolicy.CanChange(from, to));

    /// <summary>Hours are not refused, so a sheet cannot jump the supervisor and be settled.</summary>
    [Fact]
    public void ASubmittedMonthCannotBeSettledWithoutBeingApproved() =>
        Assert.False(
            TimeSheetStatusChangePolicy.CanChange(
                TimeSheetStatus.Submitted,
                TimeSheetStatus.Settled
            )
        );

    [Theory]
    [InlineData(TimeSheetStatus.Draft, true)]
    [InlineData(TimeSheetStatus.Correction, true)]
    [InlineData(TimeSheetStatus.Submitted, false)]
    [InlineData(TimeSheetStatus.Approved, false)]
    [InlineData(TimeSheetStatus.Settled, false)]
    public void OnlyADraftOrAReturnedSheetCanBeTypedInto(TimeSheetStatus status, bool editable) =>
        Assert.Equal(editable, TimeSheetStatusChangePolicy.IsEditable(status));
}
