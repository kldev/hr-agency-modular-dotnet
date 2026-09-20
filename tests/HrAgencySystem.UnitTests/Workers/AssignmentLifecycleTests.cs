using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.ChangeAssignmentStatus;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.UnitTests.Workers;

public class AssignmentLifecycleTests : BaseTest
{
    [Fact]
    public async Task Handle_AnAssignmentGoesLiveAndThenFinishes()
    {
        var assignment = WorkerScenario.Planned();

        var (live, _) = await Handle(assignment, AssignmentStatus.Active);
        assignment.Apply(live);
        Assert.Equal(AssignmentStatus.Active, assignment.Status);

        var lastDay = WorkerScenario.StartsOn.AddMonths(3);
        var (finished, _) = await Handle(assignment, AssignmentStatus.Completed, endsOn: lastDay);
        assignment.Apply(finished);

        Assert.Equal(AssignmentStatus.Completed, assignment.Status);
        Assert.Equal(lastDay, assignment.EndsOn);
    }

    /// <summary>
    /// The two ways a posting fails are different facts. Somebody who said they would go and did
    /// not is a different problem from somebody who went and came back early, and whoever plans the
    /// next crew reads them differently.
    /// </summary>
    [Fact]
    public async Task Handle_NotTurningUpIsNotTheSameAsBreakingOff()
    {
        var never = WorkerScenario.Planned();
        var (didNotStart, _) = await Handle(never, AssignmentStatus.DidNotStart);
        never.Apply(didNotStart);

        Assert.Equal(AssignmentStatus.DidNotStart, never.Status);

        var started = WorkerScenario.Planned().InStatus(AssignmentStatus.Active);
        var (interrupted, _) = await Handle(
            started,
            AssignmentStatus.Interrupted,
            endsOn: WorkerScenario.StartsOn.AddDays(10)
        );
        started.Apply(interrupted);

        Assert.Equal(AssignmentStatus.Interrupted, started.Status);
    }

    [Fact]
    public async Task Handle_SomethingThatNeverStartedCannotBeCompleted()
    {
        var assignment = WorkerScenario.Planned();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(assignment, AssignmentStatus.Completed)
        );

        Assert.Equal(ChangeAssignmentStatusHandler.TransitionNotAllowedMessage, error.Message);
    }

    [Fact]
    public async Task Handle_AFinishedAssignmentDoesNotRestart()
    {
        var assignment = WorkerScenario
            .Planned()
            .InStatus(AssignmentStatus.Active)
            .InStatus(AssignmentStatus.Completed);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(assignment, AssignmentStatus.Active)
        );
    }

    [Fact]
    public async Task Handle_EndingBeforeItStarted_ThrowsValidation()
    {
        var assignment = WorkerScenario.Planned().InStatus(AssignmentStatus.Active);

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                assignment,
                AssignmentStatus.Completed,
                endsOn: WorkerScenario.StartsOn.AddDays(-1)
            )
        );

        Assert.Contains(ChangeAssignmentStatusHandler.EndsBeforeStartMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_StartingSomebodyStillInLegalisation_Refuses()
    {
        // Planning them was fine; starting them is not. The permit is not a formality.
        var worker = WorkerScenario
            .Registered(WorkerScenario.UkrainianCitizenship)
            .InStatus(WorkerStatus.ContractPreparation)
            .InStatus(WorkerStatus.Legalisation);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(WorkerScenario.Planned(), AssignmentStatus.Active, worker: worker)
        );

        Assert.Equal(ChangeAssignmentStatusHandler.WorkerNotReadyMessage, error.Message);
    }

    [Fact]
    public async Task Handle_StartingSomebodyMidTransferIsFine()
    {
        // Arranging the move is what that stage is, and the move ends with the new posting going
        // live - so being in it cannot be what blocks it.
        var worker = WorkerScenario.Registered().Employed().InStatus(WorkerStatus.ProjectChange);

        var (result, _) = await Handle(
            WorkerScenario.Planned(),
            AssignmentStatus.Active,
            worker: worker
        );

        Assert.Equal(AssignmentStatus.Active, result.Status);
    }

    private static Task<(AssignmentStatusChanged, Wolverine.Marten.Events)> Handle(
        Assignment assignment,
        AssignmentStatus status,
        DateOnly? endsOn = null,
        Worker? worker = null
    ) =>
        ChangeAssignmentStatusHandler.Handle(
            new ChangeAssignmentStatus(
                WorkerScenario.AssignmentId,
                WorkerScenario.OrganizationId,
                status,
                endsOn,
                null,
                WorkerScenario.UserId
            ),
            assignment,
            WorkerScenario.Service(worker: worker),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
