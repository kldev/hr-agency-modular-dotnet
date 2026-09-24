using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.ReportsService.Application.Export;

/// <summary>
/// A report as a spreadsheet. Knows nothing about HTTP or SQL - it turns a finished report into
/// bytes, so the file and the screen always show the same numbers.
/// <para>
/// The first line of the first sheet is a sentence, as in the settlement export: the file leaves
/// the system and is opened with no context around it.
/// </para>
/// </summary>
public static class ReportWorkbook
{
    public const string SummarySheet = "Summary";
    public const string MonthsSheet = "Months";
    public const string SourcesSheet = "Sources";
    public const string OrganizationsSheet = "Organizations";

    public static readonly string[] MonthHeaders =
    [
        "Month",
        "Applications",
        "Interviews scheduled",
        "Offers",
        "Hires",
    ];

    public static readonly string[] SourceHeaders = ["Source", "Applications"];

    public static readonly string[] OrganizationHeaders =
    [
        "Organization",
        "Slug",
        "Job posts published",
        "Applications",
        "Interviews scheduled",
        "Offers",
        "Hires",
        "Projects went live",
        "Projects active now",
        "Last activity (UTC)",
    ];

    public static string RecruitmentFileName(RecruitmentReport report) =>
        $"recruitment-{report.From}-{report.To}.xlsx";

    public static string PlatformFileName(PlatformReport report) =>
        $"platform-{report.From}-{report.To}.xlsx";

    public static string Heading(string report, string from, string to) =>
        $"{report} for {from} – {to}. Months are calendar months in UTC.";

    public static byte[] Recruitment(RecruitmentReport report) =>
        Build(
            (SummarySheet, RecruitmentSummary(report), [34, 14]),
            (MonthsSheet, RecruitmentMonths(report), [12, 14, 22, 10, 10]),
            (SourcesSheet, Sources(report), [26, 14])
        );

    public static byte[] Platform(PlatformReport report) =>
        Build(
            (SummarySheet, PlatformSummary(report), [34, 14]),
            (OrganizationsSheet, Organizations(report), [30, 20, 18, 14, 20, 10, 10, 18, 18, 20])
        );

    private static IEnumerable<Row> RecruitmentSummary(RecruitmentReport report)
    {
        yield return new Row(
            Text(Heading("Recruitment report", report.From, report.To), Styles.Bold)
        );
        yield return new Row();
        yield return Header(["Activity in the period", ""]);
        yield return Pair("Job posts published", report.Totals.JobPostsPublished);
        yield return Pair("Applications received", report.Totals.Applications);
        yield return Pair("Interviews scheduled", report.Totals.InterviewsScheduled);
        yield return Pair("Interviews held", report.Totals.InterviewsHeld);
        yield return Pair("Offers made", report.Totals.Offers);
        yield return Pair("Hires", report.Totals.Hires);
        yield return new Row();
        yield return Header([
            "Applications received in the period, by the furthest stage reached",
            "",
        ]);
        yield return Pair("Applied", report.Funnel.Applied);
        yield return Pair("Screening", report.Funnel.Screening);
        yield return Pair("Interview", report.Funnel.Interview);
        yield return Pair("Assessment", report.Funnel.Assessment);
        yield return Pair("Offer", report.Funnel.Offer);
        yield return Pair("Hired", report.Funnel.Hired);
        yield return Pair("Rejected", report.Funnel.Rejected);
        yield return Pair("Withdrawn", report.Funnel.Withdrawn);
        yield return new Row(
            Text("Hire rate"),
            report.Funnel.HireRate is { } rate ? Number(rate, Styles.Percent) : Empty()
        );
    }

    private static IEnumerable<Row> RecruitmentMonths(RecruitmentReport report)
    {
        yield return Header(MonthHeaders);

        foreach (var month in report.Months)
        {
            yield return new Row(
                Text(month.Month),
                Number(month.Applications),
                Number(month.InterviewsScheduled),
                Number(month.Offers),
                Number(month.Hires)
            );
        }
    }

    private static IEnumerable<Row> Sources(RecruitmentReport report)
    {
        yield return Header(SourceHeaders);

        foreach (var source in report.Sources)
        {
            yield return new Row(Text(source.Source), Number(source.Applications));
        }
    }

    private static IEnumerable<Row> PlatformSummary(PlatformReport report)
    {
        yield return new Row(Text(Heading("Platform report", report.From, report.To), Styles.Bold));
        yield return new Row();
        yield return Header(["Across all organizations", ""]);
        yield return Pair("Organizations", report.Totals.Organizations);
        yield return Pair("Active in the period", report.Totals.ActiveOrganizations);
        yield return Pair("Job posts published", report.Totals.JobPostsPublished);
        yield return Pair("Applications received", report.Totals.Applications);
        yield return Pair("Interviews scheduled", report.Totals.InterviewsScheduled);
        yield return Pair("Hires", report.Totals.Hires);
        yield return Pair("Projects that went live", report.Totals.ProjectsWentLive);
    }

    private static IEnumerable<Row> Organizations(PlatformReport report)
    {
        yield return Header(OrganizationHeaders);

        foreach (var o in report.Organizations)
        {
            yield return new Row(
                Text(o.Name),
                Text(o.Slug),
                Number(o.JobPostsPublished),
                Number(o.Applications),
                Number(o.InterviewsScheduled),
                Number(o.Offers),
                Number(o.Hires),
                Number(o.ProjectsWentLive),
                Number(o.ProjectsActive),
                Text(
                    o.LastActivityAt?.UtcDateTime.ToString(
                        "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture
                    ) ?? ""
                )
            );
        }
    }

    private static byte[] Build(
        params (string Name, IEnumerable<Row> Rows, double[] Widths)[] sheets
    )
    {
        using var stream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbook = document.AddWorkbookPart();
            workbook.Workbook = new Workbook();

            var styles = workbook.AddNewPart<WorkbookStylesPart>();
            styles.Stylesheet = Styles.Stylesheet();

            var list = workbook.Workbook.AppendChild(new Sheets());

            for (var i = 0; i < sheets.Length; i++)
            {
                AddSheet(
                    workbook,
                    list,
                    (uint)i + 1,
                    sheets[i].Name,
                    sheets[i].Rows,
                    sheets[i].Widths
                );
            }

            workbook.Workbook.Save();
        }

        return stream.ToArray();
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

    private static Row Pair(string label, int value) => new(Text(label), Number(value));

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

    /// <summary>The smallest stylesheet Excel accepts - see the settlement export.</summary>
    private static class Styles
    {
        public const uint Default = 0;
        public const uint Bold = 1;
        public const uint Percent = 2;

        /// <summary>Built-in format 10 is "0.00%".</summary>
        private const uint PercentFormat = 10;

        public static Stylesheet Stylesheet() =>
            new(
                new Fonts(new Font(), new Font(new Bold())),
                new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }),
                    new Fill(new PatternFill { PatternType = PatternValues.Gray125 })
                ),
                new Borders(new Border()),
                new CellFormats(
                    new CellFormat(),
                    new CellFormat { FontId = 1, ApplyFont = true },
                    new CellFormat { NumberFormatId = PercentFormat, ApplyNumberFormat = true }
                )
            );
    }
}
