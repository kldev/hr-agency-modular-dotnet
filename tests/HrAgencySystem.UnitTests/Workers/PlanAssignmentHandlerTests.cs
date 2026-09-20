using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Application.PlanAssignment;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Workers;

public class PlanAssignmentHandlerTests : BaseTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsAssignmentPlanned()
    {
        var result = await Handle(Command());

        Assert.NotEqual(Guid.Empty, result.AssignmentId);
        Assert.Equal(WorkerScenario.WorkerId, result.WorkerId);
        Assert.Equal(WorkerScenario.ProjectId, result.Project.ProjectId);
        Assert.Equal("Backend developer", result.Position);
        Assert.Equal(EngagementType.PostingOfWorkers, result.EngagementType);
    }

    /// <summary>
    /// The snapshot is the point. An A1 is issued by a named company for a named period, so which
    /// of our companies posted somebody in 2026 has to stay readable after that company is renamed.
    /// </summary>
    [Fact]
    public async Task Handle_FreezesWhoPostedThePersonAndWhere()
    {
        var result = await Handle(Command());

        Assert.Equal(WorkerScenario.LegalEntityId, result.Project.DeliveringEntityId);
        Assert.Equal("HR Agency", result.Project.DeliveringEntityName);
        Assert.Equal("ACME Corporation", result.Project.ClientCompanyName);
        Assert.Equal("DE", result.Project.WorkCountry);
    }

    [Fact]
    public async Task Handle_CarriesTheNameAndNothingElseAboutThePerson()
    {
        // A register of postings that cannot say who was posted is not a register - but nothing
        // beyond the name travels here. The birth date and the document stay on the file.
        var result = await Handle(Command());

        Assert.Equal("Jan Kowalski", result.WorkerFullName);
    }

    [Fact]
    public async Task Handle_WithAnEndBeforeTheStart_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { EndsOn = WorkerScenario.StartsOn.AddDays(-1) })
        );

        Assert.Contains(PlanAssignmentHandler.EndsBeforeStartMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_WithoutAPosition_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { Position = "" })
        );
    }

    /// <summary>Nobody works two positions at the same time.</summary>
    [Fact]
    public async Task Handle_WhenThePersonIsAlreadyBookedOverThatPeriod_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), assignments: WorkerScenario.Assignments(overlapping: true))
        );

        Assert.Equal(PlanAssignmentHandler.OverlapsAnotherAssignmentMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithAPeriodOutsideTheProjectsOwn_Refuses()
    {
        var project = WorkerScenario.Project(
            startsOn: new DateOnly(2026, 1, 1),
            endsOn: new DateOnly(2026, 6, 30)
        );

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), project: project)
        );

        Assert.Equal(PlanAssignmentHandler.OutsideProjectPeriodMessage, error.Message);
    }

    [Fact]
    public async Task Handle_PlanningSomebodyWhoseePaperworkIsStillRunning_IsAllowed()
    {
        // This is how crews are actually scheduled: the posting is arranged while legalisation
        // works. What is not allowed is starting them, and that is the status change's job.
        var result = await Handle(
            Command(),
            worker: WorkerScenario
                .Registered(WorkerScenario.UkrainianCitizenship)
                .InStatus(WorkerStatus.ContractPreparation)
                .InStatus(WorkerStatus.Legalisation)
        );

        Assert.Equal(WorkerScenario.WorkerId, result.WorkerId);
    }

    [Fact]
    public async Task Handle_PlanningSomebodyWhoHasLeft_Refuses()
    {
        var worker = WorkerScenario.Registered().Employed().InStatus(WorkerStatus.Terminated);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), worker: worker)
        );

        Assert.Equal(PlanAssignmentHandler.WorkerNotAvailableMessage, error.Message);
    }

    private static Task<AssignmentPlanned> Handle(
        PlanAssignment command,
        Worker? worker = null,
        ProjectSnapshot? project = null,
        IAssignmentsQueryRepository? assignments = null
    ) =>
        PlanAssignmentHandler.Handle(
            command,
            WorkerScenario.Service(project, worker),
            assignments ?? WorkerScenario.Assignments(),
            Substitute.For<IDocumentSession>(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static PlanAssignment Command() =>
        new(
            WorkerScenario.OrganizationId,
            WorkerScenario.WorkerId,
            WorkerScenario.ProjectId,
            EngagementType.PostingOfWorkers,
            "Backend developer",
            WorkerScenario.StartsOn,
            null,
            WorkerScenario.UserId
        );
}
