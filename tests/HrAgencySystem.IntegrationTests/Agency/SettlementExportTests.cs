using System.Net;
using System.Net.Http.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using HrAgencySystem.Agency.Application.Employment.Start;
using HrAgencySystem.Agency.Application.TimeSheets.Export;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Endpoints.AgencyEmployment.Maps;
using HrAgencySystem.Api.Endpoints.TimeSheets.Maps;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.ValueObjects;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Agency;

/// <summary>
/// The settlement file end to end: a month approved through the real chart, then downloaded. The
/// test that matters most here is the 403 - approving somebody's hours must not show what they earn.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class SettlementExportTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    /// <summary>The first of last month: in the past, so a day may be recorded on it.</summary>
    private static readonly DateOnly LastMonth = new DateOnly(
        DateTime.UtcNow.Year,
        DateTime.UtcNow.Month,
        1
    ).AddMonths(-1);

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTimeRecords();
        await Cleaner.CleanOrgStructure();
        await Cleaner.CleanUsers();
    }

    [Fact]
    public async Task Payroll_downloads_the_month_with_the_amount_worked_out()
    {
        var agency = await ApprovedMonthAsync(Guid.NewGuid());

        var response = await Export(agency.OrganizationId, OrganizationRole.HumanResources);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            response.Content.Headers.ContentType?.MediaType
        );
        Assert.Equal(
            SettlementWorkbook.FileName(LastMonth.Year, LastMonth.Month),
            response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
        );

        var people = await PeopleIn(response);

        var row = Assert.Single(people);
        Assert.Equal("8:00", row[Column("Hours")]);
        Assert.Equal("45", row[Column("Rate")]);
        Assert.Equal("360", row[Column("Amount")]);
    }

    [Fact]
    public async Task Finance_may_download_the_file_too()
    {
        var agency = await ApprovedMonthAsync(Guid.NewGuid());

        var response = await Export(agency.OrganizationId, OrganizationRole.Finance);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// The supervisor agreed the hours and is shown neither the file nor the rate on the record:
    /// approving time is not the same as seeing pay.
    /// </summary>
    [Fact]
    public async Task The_supervisor_who_approved_the_month_is_not_shown_rates()
    {
        var agency = await ApprovedMonthAsync(Guid.NewGuid());

        var export = await Export(agency.OrganizationId, OrganizationRole.HiringManager);
        Assert.Equal(HttpStatusCode.Forbidden, export.StatusCode);

        var client = Env.CreateClient();
        client.WithOrganizationId(agency.OrganizationId);
        client.WithUserId(agency.Supervisor);
        client.SetTestRoles(nameof(OrganizationRole.HiringManager));

        var employment = await (
            await client.GetAsync($"/api/agency-employments/{agency.Worker}")
        ).ReadWithJson<AgencyEmploymentProjection>(OutputHelper);

        Assert.NotNull(employment);
        Assert.Null(employment.Rate);
    }

    /// <summary>Told apart by the rate: 45 here, 50 there, one person each.</summary>
    [Fact]
    public async Task Another_organization_does_not_see_these_people_in_its_file()
    {
        await ApprovedMonthAsync(Guid.NewGuid(), 45m);
        var theirs = await ApprovedMonthAsync(Guid.NewGuid(), 50m);

        var people = await PeopleIn(await Export(theirs.OrganizationId, OrganizationRole.Admin));

        var row = Assert.Single(people);
        Assert.Equal("400", row[Column("Amount")]);
    }

    private static int Column(string header) =>
        Array.IndexOf(SettlementWorkbook.SummaryHeaders, header);

    private sealed record ApprovedMonth(Guid OrganizationId, Guid Supervisor, Guid Worker);

    /// <summary>
    /// A board with a head and one person under them, on an hourly mandate: eight hours on
    /// one day of last month, sent in by the person and approved by their head.
    /// </summary>
    private async Task<ApprovedMonth> ApprovedMonthAsync(Guid organizationId, decimal rate = 45m)
    {
        var head = await UserClient.CreateAsync(
            organizationId,
            email: $"head-{Guid.NewGuid():N}@agency.test"
        );
        var worker = await UserClient.CreateAsync(
            organizationId,
            email: $"worker-{Guid.NewGuid():N}@agency.test"
        );

        var board = await OrgStructureClient.CreateUnitAsync(
            organizationId,
            null,
            "Board",
            OrgUnitKind.Board
        );
        await OrgStructureClient.AddMemberAsync(organizationId, board, head.Id);
        await OrgStructureClient.AssignHeadAsync(organizationId, board, head.Id);
        await OrgStructureClient.AddMemberAsync(organizationId, board, worker.Id);

        var admin = As(organizationId, head.Id, OrganizationRole.Admin);

        var started = await admin.PostAsJsonAsync(
            "/api/agency-employments",
            new MapStart.StartAgencyEmploymentRequest(
                worker.Id,
                WorkerContractType.MandateContract,
                LastMonth.AddYears(-1),
                null,
                new RateInput(rate, "PLN", RateUnit.Hourly, RateBasis.Gross)
            )
        );
        await started.Succeeded(OutputHelper);

        var self = As(organizationId, worker.Id, OrganizationRole.Recruiter);

        await Eventually.AssertAsync(async () =>
        {
            var saved = await self.PutAsJsonAsync(
                "/api/timesheets/my/days",
                new MapSaveWorkDay.SaveWorkDayRequest(LastMonth, new TimeOnly(8, 0), 8, 0, null)
            );
            await saved.Succeeded(OutputHelper);
        });

        var submitted = await self.PostAsJsonAsync(
            "/api/timesheets/my/submit",
            new MapSubmit.SubmitTimeSheetRequest(LastMonth.Year, LastMonth.Month)
        );
        await submitted.Succeeded(OutputHelper);

        var supervisor = As(organizationId, head.Id, OrganizationRole.HiringManager);

        var approved = await supervisor.PostAsJsonAsync(
            $"/api/timesheets/{worker.Id}/{LastMonth.Year}/{LastMonth.Month}/approve",
            new MapApprove.ApproveTimeSheetRequest(null)
        );
        await approved.Succeeded(OutputHelper);

        return new ApprovedMonth(organizationId, head.Id, worker.Id);
    }

    private HttpClient As(Guid organizationId, Guid userId, OrganizationRole role)
    {
        var client = Env.CreateClient();
        client.WithOrganizationId(organizationId);
        client.WithUserId(userId);
        client.SetTestRoles(role.ToString());

        return client;
    }

    private async Task<HttpResponseMessage> Export(Guid organizationId, OrganizationRole role)
    {
        var client = As(organizationId, Guid.NewGuid(), role);
        HttpResponseMessage? response = null;

        // The file reads the settlement projection, which the async daemon fills a moment later.
        await Eventually.AssertAsync(async () =>
        {
            response = await client.GetAsync(
                $"/api/timesheets/settlement/export?year={LastMonth.Year}&month={LastMonth.Month}"
            );

            if (response.StatusCode == HttpStatusCode.OK)
                Assert.NotEmpty(await PeopleIn(response));
        });

        return response!;
    }

    /// <summary>The summary rows between the header and the total.</summary>
    private static async Task<List<string[]>> PeopleIn(HttpResponseMessage response)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync();
        using var document = SpreadsheetDocument.Open(new MemoryStream(bytes), false);

        var workbook = document.WorkbookPart!;
        var sheet = workbook
            .Workbook.Sheets!.Elements<Sheet>()
            .Single(s => s.Name == SettlementWorkbook.SummarySheet);
        var rows = ((WorksheetPart)workbook.GetPartById(sheet.Id!))
            .Worksheet.GetFirstChild<SheetData>()!
            .Elements<Row>()
            .Select(row =>
                row.Elements<Cell>()
                    .Select(cell => cell.InlineString?.InnerText ?? cell.CellValue?.Text ?? "")
                    .ToArray()
            )
            .ToList();

        return rows[3..^1];
    }
}

internal static class SettlementExportHttp
{
    /// <summary>Fails with the problem details in the message, which is what a red run needs.</summary>
    public static async Task Succeeded(this HttpResponseMessage response, ITestOutputHelper output)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine(body);

        Assert.Fail($"{(int)response.StatusCode} {response.RequestMessage?.RequestUri}: {body}");
    }
}
