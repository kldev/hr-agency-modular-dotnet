using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using HrAgencySystem.Agency.Application.TimeSheets.Export;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The file itself: that it opens, where things are, and that nobody falls out of it. The
/// arithmetic is <see cref="SettlementCalculatorTests"/>; this only checks it arrived intact.
/// </summary>
public class SettlementWorkbookTests
{
    private static readonly WorkRate Hourly = new(45m, "PLN", RateUnit.Hourly, RateBasis.Gross);

    private static readonly SettlementRow Anna = Row("Anna", "Kowalska", Hourly, 480, 360m, 2);

    private static readonly SettlementRow Marek = Row("Marek", "Zielinski", null, 90, null, 1);

    [Fact]
    public void Build_HasASummaryAndADetailsSheet()
    {
        using var document = Open([Anna, Marek]);

        var names = document.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().Select(s => s.Name!.Value);

        Assert.Equal([SettlementWorkbook.SummarySheet, SettlementWorkbook.DetailsSheet], names);
    }

    [Fact]
    public void Build_OpensWithASentence_NotAHeader()
    {
        using var document = Open([Anna]);

        var first = Rows(document, SettlementWorkbook.SummarySheet)[0];

        Assert.Equal(SettlementWorkbook.Disclaimer(2026, 9), Values(first)[0]);
    }

    /// <summary>Header, two people, total - and the one without a rate is among them.</summary>
    [Fact]
    public void Build_KeepsThePersonWithoutARate()
    {
        using var document = Open([Anna, Marek]);

        var rows = Rows(document, SettlementWorkbook.SummarySheet);

        Assert.Equal(SettlementWorkbook.SummaryHeaders, Values(rows[2]));
        Assert.Equal("Marek Zielinski", Values(rows[4])[0]);
        Assert.Equal("", Values(rows[4])[AmountColumn]);
    }

    [Fact]
    public void Build_TotalMatchesTheRows()
    {
        using var document = Open([Anna, Marek]);

        var total = Values(Rows(document, SettlementWorkbook.SummarySheet)[^1]);

        Assert.Equal(SettlementWorkbook.TotalLabel, total[0]);
        Assert.Equal("9:30", total[HoursColumn]);
        Assert.Equal("360", total[AmountColumn]);
        Assert.Equal("", total[RateColumn]);
    }

    [Fact]
    public void Build_DetailsHaveOneRowPerDay()
    {
        using var document = Open([Anna, Marek]);

        var rows = Rows(document, SettlementWorkbook.DetailsSheet);

        Assert.Equal(SettlementWorkbook.DetailsHeaders, Values(rows[0]));
        Assert.Equal(1 + 3, rows.Count);
    }

    /// <summary>An end before the start reads as a typo unless the file says why.</summary>
    [Fact]
    public void Build_MarksADayThatEndsAfterMidnight()
    {
        var night = new SettlementRow(
            Person("Ola", "Adamska"),
            WorkerContractType.MandateContract,
            TimeSheetStatus.Approved,
            480,
            8m,
            null,
            null,
            [new WorkDay(new DateOnly(2026, 9, 1), new TimeOnly(22, 0), 480, "")]
        );

        using var document = Open([night]);

        var day = Values(Rows(document, SettlementWorkbook.DetailsSheet)[1]);

        Assert.Equal("22:00", day[3]);
        Assert.Equal("06:00", day[4]);
        Assert.Equal("Yes", day[5]);
    }

    [Fact]
    public void FileName_PadsTheMonth() =>
        Assert.Equal("settlement-2026-09.xlsx", SettlementWorkbook.FileName(2026, 9));

    private static readonly int HoursColumn = Array.IndexOf(SettlementWorkbook.SummaryHeaders, "Hours");
    private static readonly int RateColumn = Array.IndexOf(SettlementWorkbook.SummaryHeaders, "Rate");
    private static readonly int AmountColumn = Array.IndexOf(SettlementWorkbook.SummaryHeaders, "Amount");

    private static SpreadsheetDocument Open(IReadOnlyList<SettlementRow> rows) =>
        SpreadsheetDocument.Open(new MemoryStream(SettlementWorkbook.Build(2026, 9, rows)), false);

    private static List<Row> Rows(SpreadsheetDocument document, string sheetName)
    {
        var workbook = document.WorkbookPart!;
        var sheet = workbook.Workbook.Sheets!.Elements<Sheet>().Single(s => s.Name == sheetName);
        var part = (WorksheetPart)workbook.GetPartById(sheet.Id!);

        return [.. part.Worksheet.GetFirstChild<SheetData>()!.Elements<Row>()];
    }

    private static string[] Values(Row row) =>
        [.. row.Elements<Cell>().Select(cell => cell.InlineString?.InnerText ?? cell.CellValue?.Text ?? "")];

    private static UserSnapshot Person(string firstName, string lastName) =>
        new(Guid.NewGuid(), firstName, lastName, $"{lastName.ToLowerInvariant()}@hr-agency.com");

    private static SettlementRow Row(
        string firstName,
        string lastName,
        WorkRate? rate,
        int minutes,
        decimal? amount,
        int days
    ) =>
        new(
            Person(firstName, lastName),
            WorkerContractType.MandateContract,
            TimeSheetStatus.Approved,
            minutes,
            SettlementCalculator.DecimalHours(minutes),
            rate,
            amount,
            [
                .. Enumerable
                    .Range(1, days)
                    .Select(day => new WorkDay(
                        new DateOnly(2026, 9, day),
                        new TimeOnly(8, 0),
                        minutes / days,
                        ""
                    )),
            ]
        );
}
