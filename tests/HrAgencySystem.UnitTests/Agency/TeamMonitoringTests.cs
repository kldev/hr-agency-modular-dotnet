using HrAgencySystem.Agency.Application.TimeSheets;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The screen somebody opens every day. Approving happens once a month; chasing people happens all
/// the time, which is why the interesting rows are the ones with nothing on them.
/// </summary>
public class TeamMonitoringTests : BaseTest
{
    [Fact]
    public void SomebodyWhoHasNotStarted_StillAppears()
    {
        var rows = TeamMonitoring.Combine([Employed(OrgScenario.PayrollClerk, "Nowak")], []);

        var row = Assert.Single(rows);

        Assert.Equal(OrgScenario.PayrollClerk, row.UserId);
        Assert.False(row.HasStarted);
        Assert.Null(row.Status);
        Assert.Equal(0, row.FilledDays);
        Assert.Null(row.LastEntryOn);
    }

    [Fact]
    public void SomebodyWithASheet_CarriesTheirTotals()
    {
        var sheet = Sheet(OrgScenario.PayrollSpecialist, "Nowicka", TimeSheetStatus.Submitted);

        var rows = TeamMonitoring.Combine(
            [Employed(OrgScenario.PayrollSpecialist, "Nowicka")],
            [sheet]
        );

        var row = Assert.Single(rows);

        Assert.True(row.HasStarted);
        Assert.Equal(TimeSheetStatus.Submitted, row.Status);
        Assert.Equal(8 * 60, row.TotalMinutes);
        Assert.Equal(1, row.FilledDays);
        Assert.Equal(
            new DateOnly(TimeSheetScenario.Year, TimeSheetScenario.Month, 1),
            row.LastEntryOn
        );
    }

    /// <summary>
    /// The list is driven by who owes hours, not by which sheets happen to exist. A sheet with no
    /// matching employment belongs to somebody outside this supervisor's reach.
    /// </summary>
    [Fact]
    public void ASheetWithoutAMatchingEmployment_IsNotListed()
    {
        var stranger = Sheet(Guid.NewGuid(), "Obcy", TimeSheetStatus.Approved);

        var rows = TeamMonitoring.Combine(
            [Employed(OrgScenario.PayrollClerk, "Nowak")],
            [stranger]
        );

        var row = Assert.Single(rows);

        Assert.Equal(OrgScenario.PayrollClerk, row.UserId);
        Assert.False(row.HasStarted);
    }

    /// <summary>Everybody covered is listed, whether or not they have written anything.</summary>
    [Fact]
    public void TheStartedAndTheNotStartedSitOnOneList()
    {
        var rows = TeamMonitoring.Combine(
            [
                Employed(OrgScenario.PayrollSpecialist, "Nowicka"),
                Employed(OrgScenario.PayrollClerk, "Adamska"),
            ],
            [Sheet(OrgScenario.PayrollSpecialist, "Nowicka", TimeSheetStatus.Draft)]
        );

        Assert.Equal(2, rows.Count);

        // Sorted by surname, so the list reads the way a staff list reads.
        Assert.Equal("Adamska", rows[0].User.LastName);
        Assert.False(rows[0].HasStarted);

        Assert.Equal("Nowicka", rows[1].User.LastName);
        Assert.True(rows[1].HasStarted);
    }

    private static AgencyEmploymentProjection Employed(Guid userId, string lastName) =>
        new(
            AgencyStreamId.ForEmployment(OrgScenario.OrganizationId, userId),
            OrgScenario.OrganizationId,
            userId,
            new UserSnapshot(userId, "Anna", lastName, $"{lastName.ToLowerInvariant()}@hr.com"),
            WorkerContractType.MandateContract,
            new DateOnly(2026, 1, 1),
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            null
        );

    private static TimeSheetProjection Sheet(
        Guid userId,
        string lastName,
        TimeSheetStatus status
    ) =>
        new(
            AgencyStreamId.ForTimeSheet(
                OrgScenario.OrganizationId,
                userId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month
            ),
            OrgScenario.OrganizationId,
            userId,
            new UserSnapshot(userId, "Anna", lastName, $"{lastName.ToLowerInvariant()}@hr.com"),
            TimeSheetScenario.Year,
            TimeSheetScenario.Month,
            status,
            [new WorkDay(TimeSheetScenario.Day(1), new TimeOnly(8, 0), 8 * 60, "")],
            [],
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
}
