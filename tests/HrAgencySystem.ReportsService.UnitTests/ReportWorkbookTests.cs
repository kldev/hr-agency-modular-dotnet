using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using HrAgencySystem.ReportsService.Application.Export;
using HrAgencySystem.ReportsService.Application.Platform;
using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.ReportsService.UnitTests;

public sealed class ReportWorkbookTests
{
    private static readonly RecruitmentReport Recruitment = new(
        "2026-08",
        "2026-09",
        new RecruitmentTotals(3, 40, 12, 9, 4, 2),
        new RecruitmentFunnel(40, 25, 12, 6, 4, 2, 10, 3, 0.05m),
        [
            new RecruitmentMonth("2026-08", 15, 5, 1, 0),
            new RecruitmentMonth("2026-09", 25, 7, 3, 2),
        ],
        [new SourceCount("JustJoinIt", 30), new SourceCount("Referral", 10)]
    );

    [Fact]
    public void Recruitment_HasASheetPerSection()
    {
        using var document = Open(ReportWorkbook.Recruitment(Recruitment));

        Assert.Equal(
            [ReportWorkbook.SummarySheet, ReportWorkbook.MonthsSheet, ReportWorkbook.SourcesSheet],
            SheetNames(document)
        );
    }

    [Fact]
    public void Recruitment_MonthsAreNumbersUnderTheirHeaders()
    {
        using var document = Open(ReportWorkbook.Recruitment(Recruitment));
        var rows = Rows(document, ReportWorkbook.MonthsSheet);

        Assert.Equal(ReportWorkbook.MonthHeaders, Texts(rows[0]));
        Assert.Equal("2026-09", Texts(rows[2])[0]);

        var applications = rows[2].Elements<Cell>().ElementAt(1);
        Assert.Equal(CellValues.Number, applications.DataType!.Value);
        Assert.Equal("25", applications.CellValue!.Text);
    }

    [Fact]
    public void Recruitment_OpensWithASentenceNamingThePeriod()
    {
        using var document = Open(ReportWorkbook.Recruitment(Recruitment));
        var first = Texts(Rows(document, ReportWorkbook.SummarySheet)[0])[0];

        Assert.Equal(ReportWorkbook.Heading("Recruitment report", "2026-08", "2026-09"), first);
    }

    [Fact]
    public void Platform_ListsEveryOrganization()
    {
        var organizations = new List<OrganizationActivity>
        {
            Activity("HR Agency", applications: 20, hires: 2),
            Activity("Tech Jobs", applications: 0, hires: 0),
        };
        var report = new PlatformReport(
            "2026-04",
            "2026-09",
            PlatformReportQuery.Totals(organizations),
            organizations
        );

        using var document = Open(ReportWorkbook.Platform(report));
        var rows = Rows(document, ReportWorkbook.OrganizationsSheet);

        Assert.Equal(ReportWorkbook.OrganizationHeaders, Texts(rows[0]));
        Assert.Equal(3, rows.Count);
        Assert.Equal("platform-2026-04-2026-09.xlsx", ReportWorkbook.PlatformFileName(report));
    }

    [Fact]
    public void PlatformTotals_CountOnlyOrganizationsWithActivityAsActive()
    {
        var totals = PlatformReportQuery.Totals([
            Activity("HR Agency", applications: 20, hires: 2),
            Activity("Tech Jobs", applications: 0, hires: 0),
        ]);

        Assert.Equal(2, totals.Organizations);
        Assert.Equal(1, totals.ActiveOrganizations);
        Assert.Equal(20, totals.Applications);
        Assert.Equal(2, totals.Hires);
    }

    private static OrganizationActivity Activity(string name, int applications, int hires) =>
        new(
            Guid.NewGuid(),
            name,
            name.ToLowerInvariant().Replace(' ', '-'),
            DateTimeOffset.UnixEpoch,
            0,
            applications,
            0,
            0,
            hires,
            0,
            0,
            null
        );

    private static SpreadsheetDocument Open(byte[] bytes) =>
        SpreadsheetDocument.Open(new MemoryStream(bytes), false);

    private static List<string> SheetNames(SpreadsheetDocument document) =>
        [.. document.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().Select(s => s.Name!.Value!)];

    private static List<Row> Rows(SpreadsheetDocument document, string sheet)
    {
        var workbook = document.WorkbookPart!;
        var id = workbook.Workbook.Sheets!.Elements<Sheet>().Single(s => s.Name == sheet).Id!;
        var part = (WorksheetPart)workbook.GetPartById(id!);

        return [.. part.Worksheet.GetFirstChild<SheetData>()!.Elements<Row>()];
    }

    private static List<string> Texts(Row row) =>
        [
            .. row.Elements<Cell>()
                .Select(c => c.InlineString?.Text?.Text ?? c.CellValue?.Text ?? ""),
        ];
}
