using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.Positions;
using HrAgencySystem.Projects.Application.Positions.Archive;
using HrAgencySystem.Projects.Application.Positions.Open;
using HrAgencySystem.Projects.Application.Positions.Restore;
using HrAgencySystem.Projects.Application.Positions.Update;
using HrAgencySystem.Projects.Domain;
using Events = HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Projects;

/// <summary>
/// A role inside a project: with one client we run one project, and painters and bricklayers are
/// two positions in it. These tests cover what tells them apart and what keeps the register honest.
/// </summary>
public class ProjectPositionHandlerTests : BaseTest
{
    [Fact]
    public async Task Open_PutsTheRoleOnTheProject()
    {
        var (result, _) = await Open(ProjectScenario.Draft(), "Painter Belgium");

        Assert.Equal("Painter Belgium", result.Position.Name);
        Assert.False(result.Position.IsArchived);
        Assert.Equal(WorkerContractType.MandateContract, result.Position.ContractType);
    }

    /// <summary>
    /// Most roles are called the same thing on both sides, so the contract name is only worth
    /// typing when it differs - "Painter Belgium" internally, "Painter" on the document.
    /// </summary>
    [Fact]
    public async Task Open_WithoutAContractName_UsesTheInternalOne()
    {
        var (result, _) = await Open(ProjectScenario.Draft(), "Painter Belgium");

        Assert.Equal("Painter Belgium", result.Position.ContractName);
    }

    [Fact]
    public async Task Open_WithAContractName_KeepsBoth()
    {
        var (result, _) = await Open(
            ProjectScenario.Draft(),
            "Painter Belgium",
            contractName: "Painter"
        );

        Assert.Equal("Painter Belgium", result.Position.Name);
        Assert.Equal("Painter", result.Position.ContractName);
    }

    [Fact]
    public async Task Open_TwoRolesWithTheSameName_Refuses()
    {
        var project = await ProjectWith("Painter");

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Open(project, "painter")
        );

        Assert.Equal(OpenPositionHandler.NameAlreadyUsedMessage, error.Message);
    }

    [Fact]
    public async Task Open_TheRateNeedsItsCurrency()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Open(ProjectScenario.Draft(), "Painter", rateAmount: 32m, rateCurrency: "")
        );

        Assert.Contains(PositionDataFactory.RateCurrencyRequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Open_CollectsEveryProblemAtOnce()
    {
        // Seventeen fields, so somebody filling them in learns about all the mistakes together.
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Open(
                ProjectScenario.Draft(),
                "Painter",
                weeklyHours: 200m,
                payoutDay: 45,
                plannedHeadcount: 0
            )
        );

        Assert.Contains(PositionDataFactory.WeeklyHoursOutOfRangeMessage, error.Errors);
        Assert.Contains(PositionDataFactory.PayoutDayOutOfRangeMessage, error.Errors);
        Assert.Contains(PositionDataFactory.PlannedHeadcountOutOfRangeMessage, error.Errors);
    }

    /// <summary>One delivery often runs on several sites, so the role may carry its own address.</summary>
    [Fact]
    public async Task Open_WithoutAnAddress_LeavesTheProjectsOwn()
    {
        var (result, _) = await Open(ProjectScenario.Draft(), "Painter");

        Assert.Null(result.Position.WorkplaceAddress);
        Assert.Equal(
            ProjectScenario.Workplace,
            result.Position.WorkplaceAddressOr(ProjectScenario.Workplace)
        );
    }

    [Fact]
    public async Task Open_WithHalfAnAddress_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Open(ProjectScenario.Draft(), "Painter", street: "Rue de la Loi")
        );

        Assert.Contains(PostalAddress.CityRequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Update_ChangingTheNameSaysSoOnTheEvent()
    {
        var project = await ProjectWith("Painter");
        var positionId = project.Positions[0].PositionId;

        var (result, _) = await Update(project, positionId, "Painter Belgium");

        Assert.True(result.NameChanged);
        Assert.Equal("Painter Belgium", result.Position.Name);
    }

    [Fact]
    public async Task Update_LeavingTheNameAlone_DoesNotAnnounceARename()
    {
        var project = await ProjectWith("Painter");
        var positionId = project.Positions[0].PositionId;

        var (result, _) = await Update(project, positionId, "Painter", weeklyHours: 42m);

        Assert.False(result.NameChanged);
        Assert.Equal(42m, result.Position.WeeklyHours);
    }

    [Fact]
    public async Task Update_TakingAnotherRolesName_Refuses()
    {
        var project = await ProjectWith("Painter");
        project = await With(project, "Bricklayer");

        var painter = project.Positions[0].PositionId;

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Update(project, painter, "Bricklayer")
        );

        Assert.Equal(OpenPositionHandler.NameAlreadyUsedMessage, error.Message);
    }

    [Fact]
    public async Task Update_AnUnknownPosition_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Update(ProjectScenario.Draft(), Guid.NewGuid(), "Painter")
        );

        Assert.Equal(UpdatePositionHandler.UnknownPositionMessage, error.Message);
    }

    [Fact]
    public async Task Archive_HidesTheRoleAndFreesItsName()
    {
        var project = await ProjectWith("Painter");
        var positionId = project.Positions[0].PositionId;

        var (archived, _) = await Archive(project, positionId);
        project.Apply(archived);

        Assert.True(project.Positions[0].IsArchived);

        // Reusing the name of a role that has run its course is normal.
        var (reopened, _) = await Open(project, "Painter");
        Assert.Equal("Painter", reopened.Position.Name);
    }

    [Fact]
    public async Task Archive_Twice_Refuses()
    {
        var project = await ProjectWith("Painter");
        var positionId = project.Positions[0].PositionId;

        project.Apply((await Archive(project, positionId)).Item1);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Archive(project, positionId)
        );

        Assert.Equal(ArchivePositionHandler.AlreadyArchivedMessage, error.Message);
    }

    [Fact]
    public async Task Restore_WhenTheNameWasTakenMeanwhile_Refuses()
    {
        var project = await ProjectWith("Painter");
        var positionId = project.Positions[0].PositionId;

        project.Apply((await Archive(project, positionId)).Item1);
        project = await With(project, "Painter");

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Restore(project, positionId)
        );

        Assert.Equal(OpenPositionHandler.NameAlreadyUsedMessage, error.Message);
    }

    [Fact]
    public async Task Restore_BringsTheRoleBack()
    {
        var project = await ProjectWith("Painter");
        var positionId = project.Positions[0].PositionId;

        project.Apply((await Archive(project, positionId)).Item1);
        project.Apply((await Restore(project, positionId)).Item1);

        Assert.False(project.Positions[0].IsArchived);
    }

    private static async Task<Project> ProjectWith(string name) =>
        await With(ProjectScenario.Draft(), name);

    private static async Task<Project> With(Project project, string name)
    {
        var (opened, _) = await Open(project, name);
        project.Apply(opened);

        return project;
    }

    private static Task<(Events.ProjectPositionOpened, Wolverine.Marten.Events)> Open(
        Project project,
        string name,
        string? contractName = null,
        decimal? rateAmount = null,
        string? rateCurrency = "PLN",
        string? street = null,
        decimal? weeklyHours = null,
        int? payoutDay = null,
        int? plannedHeadcount = null
    ) =>
        OpenPositionHandler.Handle(
            new OpenPosition(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                name,
                contractName,
                "Painting facades.",
                ["Prepare the surface", "Paint"],
                ["Working at heights certificate"],
                WorkerContractType.MandateContract,
                rateAmount,
                rateCurrency,
                RateUnit.Hourly,
                RateBasis.Gross,
                street,
                null,
                null,
                null,
                null,
                null,
                weeklyHours,
                new TimeOnly(7, 0),
                "One shift",
                payoutDay,
                null,
                null,
                ["Accommodation provided"],
                plannedHeadcount,
                EngagementType.TemporaryAgencyWork,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static Task<(Events.ProjectPositionUpdated, Wolverine.Marten.Events)> Update(
        Project project,
        Guid positionId,
        string name,
        decimal? weeklyHours = null
    ) =>
        UpdatePositionHandler.Handle(
            new UpdatePosition(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                positionId,
                name,
                null,
                "Painting facades.",
                null,
                null,
                WorkerContractType.MandateContract,
                null,
                null,
                RateUnit.Hourly,
                RateBasis.Gross,
                null,
                null,
                null,
                null,
                null,
                null,
                weeklyHours,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static Task<(Events.ProjectPositionArchived, Wolverine.Marten.Events)> Archive(
        Project project,
        Guid positionId
    ) =>
        ArchivePositionHandler.Handle(
            new ArchivePosition(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                positionId,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static Task<(Events.ProjectPositionRestored, Wolverine.Marten.Events)> Restore(
        Project project,
        Guid positionId
    ) =>
        RestorePositionHandler.Handle(
            new RestorePosition(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                positionId,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
