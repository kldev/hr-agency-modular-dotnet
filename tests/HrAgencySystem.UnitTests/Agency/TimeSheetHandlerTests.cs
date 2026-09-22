using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Application.TimeSheets;
using HrAgencySystem.Agency.Application.TimeSheets.Approve;
using HrAgencySystem.Agency.Application.TimeSheets.Return;
using HrAgencySystem.Agency.Application.TimeSheets.Settle;
using HrAgencySystem.Agency.Application.TimeSheets.Submit;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// Who may decide a month. The answer comes from the chart every time it is asked, which is why
/// these run the real <c>SupervisorPolicy</c> over a real chart rather than a stubbed yes or no.
/// </summary>
public class TimeSheetHandlerTests : BaseTest
{
    private static IOrgStructureQueryRepository Chart()
    {
        var structure = OrgScenario.Company();

        var projection = new OrgStructureProjection(
            OrgStructureId.For(OrgScenario.OrganizationId),
            OrgScenario.OrganizationId,
            [
                .. structure.Units.Select(unit => new OrgUnitRow(
                    unit.UnitId,
                    unit.ParentId,
                    unit.Name,
                    unit.Kind,
                    unit.HeadUserId,
                    unit.Members,
                    unit.IsArchived
                )),
            ],
            null,
            null
        );

        var chart = Substitute.For<IOrgStructureQueryRepository>();

        chart
            .GetStructureAsync(Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>())
            .Returns(projection);

        return chart;
    }

    [Fact]
    public async Task Submit_MovesTheSheetToSubmitted()
    {
        var sheet = TimeSheetScenario.WithOneDay();

        var (@event, _) = await SubmitTimeSheetHandler.Handle(
            new SubmitTimeSheet(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                sheet.UserId
            ),
            sheet,
            OrgScenario.Service(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(8 * 60, @event.TotalMinutes);
    }

    /// <summary>Nobody sends somebody else's month - that is a separate decision, not a shortcut.</summary>
    [Fact]
    public async Task Submit_BySomebodyElse_IsRefused()
    {
        var sheet = TimeSheetScenario.WithOneDay();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            SubmitTimeSheetHandler.Handle(
                new SubmitTimeSheet(
                    OrgScenario.OrganizationId,
                    sheet.UserId,
                    TimeSheetScenario.Year,
                    TimeSheetScenario.Month,
                    OrgScenario.HeadOfPayroll
                ),
                sheet,
                OrgScenario.Service(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(SubmitTimeSheetHandler.NotYoursMessage, error.Message);
    }

    [Fact]
    public async Task Submit_AMonthWithNoHoursOnIt_IsRefused()
    {
        var sheet = TimeSheetScenario.Empty();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            SubmitTimeSheetHandler.Handle(
                new SubmitTimeSheet(
                    OrgScenario.OrganizationId,
                    sheet.UserId,
                    TimeSheetScenario.Year,
                    TimeSheetScenario.Month,
                    sheet.UserId
                ),
                sheet,
                OrgScenario.Service(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(TimeSheetRules.EmptySheetMessage, error.Message);
    }

    [Fact]
    public async Task Approve_ByTheHeadOfTheirUnit_IsAllowed()
    {
        var sheet = TimeSheetScenario.Submitted();

        var (@event, _) = await ApproveTimeSheetHandler.Handle(
            Approve(sheet.UserId, OrgScenario.HeadOfPayroll),
            sheet,
            OrgScenario.Service(),
            Chart(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(sheet.UserId, @event.UserId);
    }

    /// <summary>
    /// The head of another department is somebody's supervisor, just not this person's. This is the
    /// case a role-based check would wave through and the chart refuses.
    /// </summary>
    [Fact]
    public async Task Approve_ByAHeadFromAnotherBranch_IsRefused()
    {
        var sheet = TimeSheetScenario.Submitted();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ApproveTimeSheetHandler.Handle(
                Approve(sheet.UserId, OrgScenario.OperationsHead),
                sheet,
                OrgScenario.Service(),
                Chart(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(TimeSheetRules.NotTheSupervisorMessage, error.Message);
    }

    /// <summary>Anybody above the person counts, not just the head of their own box.</summary>
    [Fact]
    public async Task Approve_BySomebodyFurtherUpTheSameLine_IsAllowed()
    {
        var sheet = TimeSheetScenario.Submitted();

        var (@event, _) = await ApproveTimeSheetHandler.Handle(
            Approve(sheet.UserId, OrgScenario.Ceo),
            sheet,
            OrgScenario.Service(),
            Chart(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(sheet.UserId, @event.UserId);
    }

    [Fact]
    public async Task Approve_ByThePersonThemselves_IsRefused()
    {
        var sheet = TimeSheetScenario.Submitted();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ApproveTimeSheetHandler.Handle(
                Approve(sheet.UserId, sheet.UserId),
                sheet,
                OrgScenario.Service(),
                Chart(),
                TestClock,
                CancellationToken.None
            )
        );
    }

    /// <summary>A month handed back without a reason comes straight back.</summary>
    [Fact]
    public async Task Return_WithoutSayingWhat_IsRefused()
    {
        var sheet = TimeSheetScenario.Submitted();

        await Assert.ThrowsAsync<ValidationException>(() =>
            ReturnTimeSheetForCorrectionHandler.Handle(
                new ReturnTimeSheetForCorrection(
                    OrgScenario.OrganizationId,
                    sheet.UserId,
                    TimeSheetScenario.Year,
                    TimeSheetScenario.Month,
                    "   ",
                    false,
                    OrgScenario.HeadOfPayroll
                ),
                sheet,
                OrgScenario.Service(),
                Chart(),
                TestClock,
                CancellationToken.None
            )
        );
    }

    /// <summary>The reason rides on the event, so the thread cannot end up without it.</summary>
    [Fact]
    public async Task Return_PutsTheReasonOnTheThread()
    {
        var sheet = TimeSheetScenario.Submitted();

        var (@event, _) = await ReturnTimeSheetForCorrectionHandler.Handle(
            new ReturnTimeSheetForCorrection(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                "The last week is missing.",
                false,
                OrgScenario.HeadOfPayroll
            ),
            sheet,
            OrgScenario.Service(),
            Chart(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal("The last week is missing.", @event.Reason.Content);
        Assert.Equal(TimeSheetRole.Supervisor, @event.Reason.AuthorRole);

        sheet.Apply(@event);

        Assert.Equal(TimeSheetStatus.Correction, sheet.Status);
        Assert.Single(sheet.Comments);
        Assert.Null(sheet.ApprovedAt);
    }

    /// <summary>Payroll may hand back what it has already been given, without being anybody's boss.</summary>
    [Fact]
    public async Task Return_ByPayroll_NeedsNoPlaceInTheChart()
    {
        var sheet = TimeSheetScenario.Approved();

        var (@event, _) = await ReturnTimeSheetForCorrectionHandler.Handle(
            new ReturnTimeSheetForCorrection(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                "The rate period does not match.",
                true,
                OrgScenario.PosterAbroad
            ),
            sheet,
            OrgScenario.Service(),
            Chart(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(TimeSheetRole.Payroll, @event.Reason.AuthorRole);
    }

    [Fact]
    public async Task Settle_ByAnybodyButPayroll_IsRefused()
    {
        var sheet = TimeSheetScenario.Approved();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            SettleTimeSheetHandler.Handle(
                new SettleTimeSheet(
                    OrgScenario.OrganizationId,
                    sheet.UserId,
                    TimeSheetScenario.Year,
                    TimeSheetScenario.Month,
                    false,
                    OrgScenario.HeadOfPayroll
                ),
                sheet,
                OrgScenario.Service(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(TimeSheetRules.NotPayrollMessage, error.Message);
    }

    [Fact]
    public async Task Settle_AMonthNobodyApproved_IsRefused()
    {
        var sheet = TimeSheetScenario.Submitted();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            SettleTimeSheetHandler.Handle(
                new SettleTimeSheet(
                    OrgScenario.OrganizationId,
                    sheet.UserId,
                    TimeSheetScenario.Year,
                    TimeSheetScenario.Month,
                    true,
                    OrgScenario.HeadOfPayroll
                ),
                sheet,
                OrgScenario.Service(),
                TestClock,
                CancellationToken.None
            )
        );
    }

    private static ApproveTimeSheet Approve(Guid userId, Guid approvedBy) =>
        new(
            OrgScenario.OrganizationId,
            userId,
            TimeSheetScenario.Year,
            TimeSheetScenario.Month,
            null,
            approvedBy
        );
}
