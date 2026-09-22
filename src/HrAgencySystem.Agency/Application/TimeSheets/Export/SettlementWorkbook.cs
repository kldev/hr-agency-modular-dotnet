using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace HrAgencySystem.Agency.Application.TimeSheets.Export;

/// <summary>
/// The settlement month as a spreadsheet: a summary with one row per person and the days behind
/// it. Knows nothing about HTTP - it turns rows into bytes, and the numbers in them come from
/// <see cref="SettlementCalculator"/>, never from here.
/// <para>
/// The first line of the summary is a sentence rather than a header. The file leaves the system
/// and will be opened with no context around it, and that sentence is the only thing that survives
/// being forwarded by mail.
/// </para>
/// </summary>
public static class SettlementWorkbook
{
    public const string SummarySheet = "Summary";
    public const string DetailsSheet = "Details";
    public const string TotalLabel = "TOTAL";

    public static readonly string[] SummaryHeaders =
    [
        "Person",
        "Email",
        "Contract",
        "Status",
        "Hours",
        "Decimal hours",
        "Rate",
        "Per",
        "Basis",
        "Currency",
        "Amount",
    ];

    public static readonly string[] DetailsHeaders =
    [
        "Person",
        "Date",
        "Day",
        "Start",
        "End",
        "Ends next day",
        "Hours",
        "Minutes",
        "Decimal hours",
        "Note",
    ];

    public static string Disclaimer(int year, int month) =>
        $"Working hours agreed for {TimeSheetPeriodLabel.For(year, month)}. "
        + "Amounts are hours × hourly rate before any deductions — not a payslip.";

    public static string FileName(int year, int month) =>
        string.Create(CultureInfo.InvariantCulture, $"settlement-{year}-{month:00}.xlsx");

    public static byte[] Build(int year, int month, IReadOnlyList<SettlementRow> rows)
    {
        using var stream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbook = document.AddWorkbookPart();
            workbook.Workbook = new Workbook();

            var styles = workbook.AddNewPart<WorkbookStylesPart>();
            styles.Stylesheet = Styles.Stylesheet();

            var sheets = workbook.Workbook.AppendChild(new Sheets());

            AddSheet(workbook, sheets, 1, SummarySheet, Summary(year, month, rows), SummaryWidths);
            AddSheet(workbook, sheets, 2, DetailsSheet, Details(rows), DetailsWidths);

            workbook.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static readonly double[] SummaryWidths = [28, 32, 26, 12, 10, 14, 10, 10, 8, 10, 14];

    private static readonly double[] DetailsWidths = [28, 12, 12, 8, 8, 14, 8, 8, 14, 48];

    private static IEnumerable<Row> Summary(int year, int month, IReadOnlyList<SettlementRow> rows)
    {
        yield return new Row(Text(Disclaimer(year, month), Styles.Bold));
        yield return new Row();
        yield return Header(SummaryHeaders);

        foreach (var row in rows)
        {
            yield return new Row(
                Text(NameOf(row)),
                Text(row.User.Email),
                Text(row.ContractType?.ToString() ?? ""),
                Text(row.Status.ToString()),
                Text(Duration(row.Minutes)),
                Number(row.DecimalHours, Styles.Decimal),
                row.Rate is null ? Empty() : Number(row.Rate.Amount, Styles.Decimal),
                Text(row.Rate?.Unit.ToString() ?? ""),
                Text(row.Rate?.Basis.ToString() ?? ""),
                Text(row.Rate?.Currency ?? ""),
                row.Amount is { } amount ? Number(amount, Styles.Decimal) : Empty()
            );
        }

        // The rate column stays empty on this line: a sum of rates is not anything.
        var total = SettlementCalculator.Total(rows);

        yield return new Row(
            Text(TotalLabel, Styles.Bold),
            Empty(),
            Empty(),
            Empty(),
            Text(Duration(total.Minutes), Styles.Bold),
            Number(total.DecimalHours, Styles.BoldDecimal),
            Empty(),
            Empty(),
            Empty(),
            Text(total.Currency ?? "", Styles.Bold),
            total.Amount is { } sum ? Number(sum, Styles.BoldDecimal) : Empty()
        );
    }

    private static IEnumerable<Row> Details(IReadOnlyList<SettlementRow> rows)
    {
        yield return Header(DetailsHeaders);

        foreach (var row in rows)
        foreach (var day in row.Days)
        {
            yield return new Row(
                Text(NameOf(row)),
                Number((decimal)day.Date.ToDateTime(TimeOnly.MinValue).ToOADate(), Styles.Date),
                Text(day.Date.DayOfWeek.ToString()),
                Text(day.StartsAt.ToString("HH:mm", CultureInfo.InvariantCulture)),
                Text(day.EndsAt.ToString("HH:mm", CultureInfo.InvariantCulture)),
                // Marked, because an end earlier than the start otherwise reads as a typo.
                Text(day.CrossesMidnight ? "Yes" : ""),
                Number(day.Minutes / 60),
                Number(day.Minutes % 60),
                Number(SettlementCalculator.DecimalHours(day.Minutes), Styles.Decimal),
                Text(day.Note)
            );
        }
    }

    private static void AddSheet(
        WorkbookPart workbook,
        Sheets sheets,
        uint id,
        string name,
        IEnumerable<Row> rows,
        double[] widths
    )
    {
        var part = workbook.AddNewPart<WorksheetPart>();

        var columns = new Columns(
            widths.Select(
                (width, index) =>
                    new Column
                    {
                        Min = (uint)index + 1,
                        Max = (uint)index + 1,
                        Width = width,
                        CustomWidth = true,
                    }
            )
        );

        part.Worksheet = new Worksheet(columns, new SheetData(rows));

        sheets.AppendChild(
            new Sheet
            {
                Id = workbook.GetIdOfPart(part),
                SheetId = id,
                Name = name,
            }
        );
    }

    private static string NameOf(SettlementRow row) => $"{row.User.FirstName} {row.User.LastName}";

    /// <summary>"161:30" - hours past 24 are the whole point, so this is not a time of day.</summary>
    private static string Duration(int minutes) =>
        string.Create(CultureInfo.InvariantCulture, $"{minutes / 60}:{minutes % 60:00}");

    private static Row Header(IEnumerable<string> headers) =>
        new(headers.Select(header => Text(header, Styles.Bold)));

    private static Cell Text(string value, uint style = Styles.Default) =>
        new()
        {
            DataType = CellValues.InlineString,
            InlineString = new InlineString(new Text(value)),
            StyleIndex = style,
        };

    private static Cell Number(decimal value, uint style = Styles.Default) =>
        new()
        {
            DataType = CellValues.Number,
            CellValue = new CellValue(value),
            StyleIndex = style,
        };

    private static Cell Empty() => new();

    /// <summary>
    /// The smallest stylesheet Excel accepts: it insists on two fills and on a font, a border and
    /// a format existing at index zero before it will look at anything else.
    /// </summary>
    private static class Styles
    {
        public const uint Default = 0;
        public const uint Bold = 1;
        public const uint Decimal = 2;
        public const uint BoldDecimal = 3;
        public const uint Date = 4;

        /// <summary>Built-in formats: 2 is "0.00", 14 is the short date.</summary>
        private const uint TwoDecimals = 2;
        private const uint ShortDate = 14;

        public static Stylesheet Stylesheet() =>
            new(
                new Fonts(new Font(), new Font(new DocumentFormat.OpenXml.Spreadsheet.Bold())),
                new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }),
                    new Fill(new PatternFill { PatternType = PatternValues.Gray125 })
                ),
                new Borders(new Border()),
                new CellFormats(
                    new CellFormat(),
                    new CellFormat { FontId = 1, ApplyFont = true },
                    new CellFormat { NumberFormatId = TwoDecimals, ApplyNumberFormat = true },
                    new CellFormat
                    {
                        FontId = 1,
                        ApplyFont = true,
                        NumberFormatId = TwoDecimals,
                        ApplyNumberFormat = true,
                    },
                    new CellFormat { NumberFormatId = ShortDate, ApplyNumberFormat = true }
                )
            );
    }
}
