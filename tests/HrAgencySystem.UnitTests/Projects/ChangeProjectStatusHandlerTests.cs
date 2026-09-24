using HrAgencySystem.Projects.Application.ChangeStatus;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Projects;

public class ChangeProjectStatusHandlerTests : BaseTest
{
    [Fact]
    public async Task Handle_DraftToActive_WhenReady_ReturnsStatusChanged()
    {
        var project = ProjectScenario.Draft().WithResponsible().WithSignedContract();

        var (result, events) = await Handle(project, ProjectStatus.Active);

        Assert.Equal(ProjectStatus.Draft, result.PreviousStatus);
        Assert.Equal(ProjectStatus.Active, result.Status);
        Assert.Single(events);
    }

    [Fact]
    public async Task Handle_DraftToActive_WithoutASignedContract_Refuses()
    {
        var project = ProjectScenario.Draft().WithResponsible();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(project, ProjectStatus.Active)
        );

        Assert.Equal(ChangeProjectStatusHandler.ContractRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Handle_DraftToActive_WithoutAResponsibleContact_Refuses()
    {
        var project = ProjectScenario.Draft().WithSignedContract();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(project, ProjectStatus.Active)
        );

        Assert.Equal(ChangeProjectStatusHandler.ResponsibleRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Handle_DraftToActive_WithAnIncompleteClientProfile_Refuses()
    {
        var project = ProjectScenario.Draft().WithResponsible().WithSignedContract();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                project,
                ProjectStatus.Active,
                ProjectScenario.Service(ProjectScenario.IncompleteCompany)
            )
        );

        Assert.Equal(ChangeProjectStatusHandler.CompanyProfileRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ReadsTheClientProfileLiveRatherThanFromCreation()
    {
        // The snapshot on the aggregate is what the company looked like when the project was
        // drafted. Completing the profile afterwards is the normal order of events, so the check
        // has to ask the company module now.
        var project = ProjectScenario.Draft().WithResponsible().WithSignedContract();
        var service = ProjectScenario.Service(ProjectScenario.CompleteCompany);

        var (result, _) = await Handle(project, ProjectStatus.Active, service);

        Assert.Equal(ProjectStatus.Active, result.Status);
        await service
            .Received(1)
            .GetCompanyAsync(
                Arg.Any<OrganizationId>(),
                ProjectScenario.CompanyId,
                Arg.Any<CancellationToken>()
            );
    }

    [Theory]
    [InlineData(ProjectStatus.Suspended)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.Cancelled)]
    public async Task Handle_FromActive_AllowsTheExpectedMoves(ProjectStatus target)
    {
        var project = ProjectScenario.Draft().Live();

        var (result, _) = await Handle(project, target);

        Assert.Equal(target, result.Status);
    }

    [Fact]
    public async Task Handle_SuspendedBackToActive_IsAllowed()
    {
        var project = ProjectScenario.Draft().Live().InStatus(ProjectStatus.Suspended);

        var (result, _) = await Handle(project, ProjectStatus.Active);

        Assert.Equal(ProjectStatus.Active, result.Status);
    }

    [Theory]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.Cancelled)]
    public async Task Handle_FromAFinalStatus_Refuses(ProjectStatus final)
    {
        var project = ProjectScenario.Draft().Live().InStatus(final);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(project, ProjectStatus.Active)
        );

        Assert.Equal(ChangeProjectStatusHandler.TransitionNotAllowedMessage, error.Message);
    }

    [Fact]
    public async Task Handle_DraftStraightToCompleted_Refuses()
    {
        var project = ProjectScenario.Draft();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(project, ProjectStatus.Completed)
        );

        Assert.Equal(ChangeProjectStatusHandler.TransitionNotAllowedMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ToTheSameStatus_Refuses()
    {
        var project = ProjectScenario.Draft();

        await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(project, ProjectStatus.Draft));
    }

    [Fact]
    public async Task Handle_ChecksTheTenantBeforeTouchingTheAggregate()
    {
        // The handler's job is to ask; refusing is the service's job, and ProjectServiceTests covers
        // that. Asserting a throw here would only prove that a substitute was configured to throw.
        var project = ProjectScenario.Draft();
        var service = ProjectScenario.Service();

        await Handle(project, ProjectStatus.Cancelled, service);

        service.Received(1).ValidateAggregateUpdate(project, ProjectScenario.OrganizationId);
    }

    [Fact]
    public async Task Handle_KeepsTheReasonOnTheEvent()
    {
        var project = ProjectScenario.Draft();

        var (result, _) = await Handle(
            project,
            ProjectStatus.Cancelled,
            reason: " Client pulled out "
        );

        Assert.Equal("Client pulled out", result.Reason);
    }

    private static Task<(ProjectStatusChanged, Wolverine.Marten.Events)> Handle(
        Project project,
        ProjectStatus status,
        IProjectService? service = null,
        string? reason = null
    ) =>
        ChangeProjectStatusHandler.Handle(
            new ChangeProjectStatus(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                status,
                reason,
                ProjectScenario.UserId
            ),
            project,
            service ?? ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
