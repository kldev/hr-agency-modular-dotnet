using HrAgencySystem.Agency.Application.TimeSheets.Export;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The arithmetic behind the settlement file. Every case here is one somebody would check with a
/// calculator before making a transfer.
/// </summary>
public class SettlementCalculatorTests
{
    private static WorkRate Hourly(decimal amount, string currency = "PLN") =>
        new(amount, currency, RateUnit.Hourly, RateBasis.Gross);

    [Fact]
    public void AmountFor_AnHourlyRate_MultipliesTheHours() =>
        Assert.Equal(360.00m, SettlementCalculator.AmountFor(8 * 60, Hourly(45m)));

    /// <summary>1 h 40 min is 1.67 hours on the page, but it is paid as 5/3 of an hour.</summary>
    [Fact]
    public void AmountFor_IsWorkedOutFromMinutes_NotFromRoundedHours() =>
        Assert.Equal(75.00m, SettlementCalculator.AmountFor(100, Hourly(45m)));

    [Theory]
    [InlineData(RateUnit.Monthly)]
    [InlineData(RateUnit.Daily)]
    public void AmountFor_ARateThatIsNotHourly_IsEmpty(RateUnit unit) =>
        Assert.Null(
            SettlementCalculator.AmountFor(160 * 60, new WorkRate(8500m, "PLN", unit, RateBasis.Gross))
        );

    [Fact]
    public void AmountFor_NoRate_IsEmpty() => Assert.Null(SettlementCalculator.AmountFor(480, null));

    /// <summary>30 minutes at 0.01 is half a grosz; banker's rounding would make it nothing.</summary>
    [Fact]
    public void AmountFor_HalfAGrosz_RoundsAwayFromZero() =>
        Assert.Equal(0.01m, SettlementCalculator.AmountFor(30, Hourly(0.01m)));

    [Fact]
    public void DecimalHours_RoundsToTwoPlaces() =>
        Assert.Equal(1.67m, SettlementCalculator.DecimalHours(100));

    /// <summary>
    /// Three days of 20 minutes at 1.00: rounding each day gives 3 × 0.33 = 0.99, rounding the
    /// month gives 1.00. The month is what gets paid.
    /// </summary>
    [Fact]
    public void Rows_RoundOnceOnTheMonth_NotPerDay()
    {
        var sheet = Sheet(Person("Anna", "Kowalska"), Day(1, 20), Day(2, 20), Day(3, 20));

        var row = Assert.Single(
            SettlementCalculator.Rows([sheet], [Employment(sheet.UserId, Hourly(1m))])
        );

        Assert.Equal(1.00m, row.Amount);
    }

    /// <summary>The person nobody quoted a rate for is the one payroll most needs to see.</summary>
    [Fact]
    public void Rows_SomebodyWithoutARate_IsKeptWithNoAmount()
    {
        var sheet = Sheet(Person("Anna", "Kowalska"), Day(1, 480));

        var row = Assert.Single(SettlementCalculator.Rows([sheet], [Employment(sheet.UserId, null)]));

        Assert.Equal(480, row.Minutes);
        Assert.Null(row.Rate);
        Assert.Null(row.Amount);
    }

    [Fact]
    public void Rows_SomebodyWithNoEmploymentRecord_IsKept()
    {
        var sheet = Sheet(Person("Anna", "Kowalska"), Day(1, 480));

        var row = Assert.Single(SettlementCalculator.Rows([sheet], []));

        Assert.Null(row.ContractType);
        Assert.Null(row.Amount);
    }

    [Fact]
    public void Rows_AreOrderedByLastName()
    {
        var zielinski = Sheet(Person("Marek", "Zielinski"), Day(1, 60));
        var adamska = Sheet(Person("Ola", "Adamska"), Day(1, 60));

        var rows = SettlementCalculator.Rows([zielinski, adamska], []);

        Assert.Equal(["Adamska", "Zielinski"], rows.Select(row => row.User.LastName));
    }

    [Fact]
    public void Total_AddsHoursAndAmounts()
    {
        var anna = Sheet(Person("Anna", "Kowalska"), Day(1, 480));
        var ola = Sheet(Person("Ola", "Adamska"), Day(1, 90));
        var marek = Sheet(Person("Marek", "Zielinski"), Day(1, 60));

        var total = SettlementCalculator.Total(
            SettlementCalculator.Rows(
                [anna, ola, marek],
                [
                    Employment(anna.UserId, Hourly(45m)),
                    Employment(ola.UserId, Hourly(40m)),
                    Employment(marek.UserId, null),
                ]
            )
        );

        Assert.Equal(630, total.Minutes);
        Assert.Equal(10.50m, total.DecimalHours);
        Assert.Equal(360m + 60m, total.Amount);
        Assert.Equal("PLN", total.Currency);
    }

    /// <summary>Zloty plus euro is a number, not an amount anybody can transfer.</summary>
    [Fact]
    public void Total_InMoreThanOneCurrency_HasNoAmount()
    {
        var anna = Sheet(Person("Anna", "Kowalska"), Day(1, 60));
        var ola = Sheet(Person("Ola", "Adamska"), Day(1, 60));

        var total = SettlementCalculator.Total(
            SettlementCalculator.Rows(
                [anna, ola],
                [
                    Employment(anna.UserId, Hourly(45m)),
                    Employment(ola.UserId, Hourly(12m, "EUR")),
                ]
            )
        );

        Assert.Equal(120, total.Minutes);
        Assert.Null(total.Amount);
        Assert.Null(total.Currency);
    }

    private static UserSnapshot Person(string firstName, string lastName) =>
        new(Guid.NewGuid(), firstName, lastName, $"{lastName.ToLowerInvariant()}@hr-agency.com");

    private static WorkDay Day(int day, int minutes) =>
        new(TimeSheetScenario.Day(day), new TimeOnly(8, 0), minutes, "");

    private static TimeSheetProjection Sheet(UserSnapshot user, params WorkDay[] days) =>
        new(
            Guid.NewGuid(),
            OrgScenario.OrganizationId,
            user.Id,
            user,
            TimeSheetScenario.Year,
            TimeSheetScenario.Month,
            TimeSheetStatus.Approved,
            days,
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

    private static AgencyEmploymentProjection Employment(Guid userId, WorkRate? rate) =>
        new(
            Guid.NewGuid(),
            OrgScenario.OrganizationId,
            userId,
            new UserSnapshot(userId, "Some", "Body", "some.body@hr-agency.com"),
            WorkerContractType.MandateContract,
            new DateOnly(2026, 1, 1),
            null,
            null,
            rate,
            DateTimeOffset.UtcNow,
            null,
            null
        );
}
