using HrAgencySystem.Agency.Application.TimeSheets.Approve;
using HrAgencySystem.Agency.Application.TimeSheets.Return;
using HrAgencySystem.Agency.Application.TimeSheets.Settle;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.SharedKernel.Snapshots;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// Who hears about a decision on their month.
/// <para>
/// These use a service double that answers with a <em>different</em> person per id, unlike
/// <see cref="OrgScenario.Service"/>, which hands back the same snapshot to everybody. That
/// difference is the whole subject here: the rule being tested is "the person who decided is not
/// the person who is told", and a double that makes everybody the same person cannot show it.
/// </para>
/// </summary>
public class TimeSheetMailTests : BaseTest
{
    private static IAgencyService PeopleService()
    {
        var service = Substitute.For<IAgencyService>();

        service
            .GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Person(callInfo.ArgAt<Guid>(0)));

        return service;
    }

    private static UserSnapshot Person(Guid id) =>
        new(id, "Person", id.ToString()[..4], $"{id.ToString()[..4]}@hr-agency.com");

    [Fact]
    public async Task Approving_TellsTheOwnerWhoApprovedAndWhatTheySaid()
    {
        var sheet = TimeSheetScenario.Submitted(OrgScenario.PayrollSpecialist);

        var (_, _, messages) = await ApproveTimeSheetHandler.Handle(
            new ApproveTimeSheet(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                "Thanks, all clear.",
                OrgScenario.HeadOfPayroll
            ),
            sheet,
            PeopleService(),
            OrgScenario.Chart(),
            TestClock,
            CancellationToken.None
        );

        var mail = Assert.Single(messages.OfType<SendTimeSheetApproved>());

        Assert.Equal(Person(sheet.UserId).Email, mail.RecipientEmail);
        Assert.Equal(Person(OrgScenario.HeadOfPayroll).Fullname, mail.ApprovedByFullname);
        Assert.Equal("Thanks, all clear.", mail.Comment);
        Assert.Equal("September 2026", mail.Period);
    }

    /// <summary>An approval with nothing said carries an empty note, never a missing one.</summary>
    [Fact]
    public async Task Approving_WithoutANote_StillTellsTheOwner()
    {
        var sheet = TimeSheetScenario.Submitted(OrgScenario.PayrollSpecialist);

        var (_, _, messages) = await ApproveTimeSheetHandler.Handle(
            new ApproveTimeSheet(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                null,
                OrgScenario.HeadOfPayroll
            ),
            sheet,
            PeopleService(),
            OrgScenario.Chart(),
            TestClock,
            CancellationToken.None
        );

        var mail = Assert.Single(messages.OfType<SendTimeSheetApproved>());

        Assert.Equal("", mail.Comment);
    }

    [Fact]
    public async Task SendingBack_CarriesTheReasonAndWhoWoreWhichHat()
    {
        var sheet = TimeSheetScenario.Submitted(OrgScenario.PayrollSpecialist);

        var (_, _, messages) = await ReturnTimeSheetForCorrectionHandler.Handle(
            new ReturnTimeSheetForCorrection(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                "The 14th is missing.",
                false,
                OrgScenario.HeadOfPayroll
            ),
            sheet,
            PeopleService(),
            OrgScenario.Chart(),
            TestClock,
            CancellationToken.None
        );

        var mail = Assert.Single(messages.OfType<SendTimeSheetReturnedForCorrection>());

        Assert.Equal("The 14th is missing.", mail.Reason);
        Assert.Equal(nameof(TimeSheetRole.Supervisor), mail.ReturnedByRole);
    }

    /// <summary>
    /// Payroll may hand back its own month, and then there is nobody to tell. This is one of the two
    /// places the rule stops a real mail rather than merely restating itself.
    /// </summary>
    [Fact]
    public async Task SendingBackYourOwnMonth_TellsNobody()
    {
        var sheet = TimeSheetScenario.Submitted(OrgScenario.PayrollSpecialist);

        var (_, _, messages) = await ReturnTimeSheetForCorrectionHandler.Handle(
            new ReturnTimeSheetForCorrection(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                "Mine, and I got it wrong.",
                true,
                sheet.UserId
            ),
            sheet,
            PeopleService(),
            OrgScenario.Chart(),
            TestClock,
            CancellationToken.None
        );

        Assert.Empty(messages.OfType<SendTimeSheetReturnedForCorrection>());
    }

    [Fact]
    public async Task Settling_TellsTheOwnerWhoClosedTheMonth()
    {
        var sheet = TimeSheetScenario.Approved(OrgScenario.PayrollSpecialist);

        var (_, _, messages) = await SettleTimeSheetHandler.Handle(
            new SettleTimeSheet(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                true,
                OrgScenario.HeadOfPayroll
            ),
            sheet,
            PeopleService(),
            TestClock,
            CancellationToken.None
        );

        var mail = Assert.Single(messages.OfType<SendTimeSheetSettled>());

        Assert.Equal(Person(sheet.UserId).Email, mail.RecipientEmail);
        Assert.Equal(Person(OrgScenario.HeadOfPayroll).Fullname, mail.SettledByFullname);
    }

    /// <summary>Payroll settles everybody, itself included - and hears nothing about its own.</summary>
    [Fact]
    public async Task SettlingYourOwnMonth_TellsNobody()
    {
        var sheet = TimeSheetScenario.Approved(OrgScenario.PayrollSpecialist);

        var (_, _, messages) = await SettleTimeSheetHandler.Handle(
            new SettleTimeSheet(
                OrgScenario.OrganizationId,
                sheet.UserId,
                TimeSheetScenario.Year,
                TimeSheetScenario.Month,
                true,
                sheet.UserId
            ),
            sheet,
            PeopleService(),
            TestClock,
            CancellationToken.None
        );

        Assert.Empty(messages.OfType<SendTimeSheetSettled>());
    }
}
