using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Application.TimeSheets.SaveWorkDay;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// Writing one day. Most days carry no note at all, which is the case the seeder tripped over.
/// </summary>
public class SaveWorkDayHandlerTests : BaseTest
{
    private static IAgencyEmploymentQueryRepository Employments()
    {
        var employments = Substitute.For<IAgencyEmploymentQueryRepository>();

        employments
            .GetAsync(Arg.Any<OrganizationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(
                new AgencyEmploymentProjection(
                    AgencyStreamId.ForEmployment(
                        OrgScenario.OrganizationId,
                        OrgScenario.PayrollSpecialist
                    ),
                    OrgScenario.OrganizationId,
                    OrgScenario.PayrollSpecialist,
                    TimeSheetScenario.Owner,
                    WorkerContractType.EmploymentContract,
                    new DateOnly(2020, 1, 1),
                    null,
                    40m,
                    DateTimeOffset.UtcNow,
                    null,
                    null
                )
            );

        return employments;
    }

    private static SaveWorkDay Command(DateOnly date, string? note) =>
        new(
            OrgScenario.OrganizationId,
            OrgScenario.PayrollSpecialist,
            date.Year,
            date.Month,
            date,
            new TimeOnly(8, 0),
            8,
            0,
            note,
            OrgScenario.PayrollSpecialist
        );

    private async Task<WorkDaySaved> Save(string? note)
    {
        var today = DateOnly.FromDateTime(TestClock.UtcNow.UtcDateTime);

        return await SaveWorkDayHandler.Handle(
            Command(today, note),
            OrgScenario.Service(),
            Employments(),
            Substitute.For<IDocumentSession>(),
            TestClock,
            CancellationToken.None
        );
    }

    /// <summary>A day is hours, not a story - saying nothing about it is the normal case.</summary>
    [Fact]
    public async Task Save_WithoutANote_IsAccepted()
    {
        var saved = await Save(null);

        Assert.Equal("", saved.Day.Note);
        Assert.Equal(8 * 60, saved.Day.Minutes);
    }

    [Fact]
    public async Task Save_WithANote_KeepsIt()
    {
        var saved = await Save("  Worked from the client's office.  ");

        Assert.Equal("Worked from the client's office.", saved.Day.Note);
    }

    [Fact]
    public async Task Save_WithATooLongNote_IsRefused()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Save(new string('x', ShortNote.MaxLength + 1))
        );

        Assert.Contains(ShortNote.MaxLengthMessage, error.Message);
    }
}
