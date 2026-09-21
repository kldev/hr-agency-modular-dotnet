using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.ChangeAssignmentStatus;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.UnitTests.Workers;

public class AssignmentLifecycleTests : BaseTest
{
    [Fact]
    public async Task Handle_AnAssignmentGoesLiveAndThenFinishes()
    {
        var assignment = WorkerScenario.Planned();

        var (live, _, _) = await Handle(assignment, AssignmentStatus.Active);
        assignment.Apply(live);
        Assert.Equal(AssignmentStatus.Active, assignment.Status);

        var lastDay = WorkerScenario.StartsOn.AddMonths(3);
        var (finished, _, _) = await Handle(
            assignment,
            AssignmentStatus.Completed,
            endsOn: lastDay
        );
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
        var (didNotStart, _, _) = await Handle(never, AssignmentStatus.DidNotStart);
        never.Apply(didNotStart);

        Assert.Equal(AssignmentStatus.DidNotStart, never.Status);

        var started = WorkerScenario.Planned().InStatus(AssignmentStatus.Active);
        var (interrupted, _, _) = await Handle(
            started,
            AssignmentStatus.Interrupted,
            endsOn: WorkerScenario.StartsOn.AddDays(10)
        );
        started.Apply(interrupted);

        Assert.Equal(AssignmentStatus.Interrupted, started.Status);
    }

    /// <summary>
    /// The seat on the role is held from the moment somebody is planned onto it until the posting
    /// ends, one way or another. Going live changes nothing about that, which is the case worth
    /// pinning down: the count is of people held against the role, not of people on site.
    /// </summary>
    [Fact]
    public async Task Handle_TheRoleIsOnlyFreedWhenThePostingEnds()
    {
        var assignment = WorkerScenario.Planned();

        var (live, _, whileLive) = await Handle(assignment, AssignmentStatus.Active);
        Assert.Empty(whileLive.OfType<AssignmentPositionUnstaffed>());

        assignment.Apply(live);

        var (_, _, whenFinished) = await Handle(
            assignment,
            AssignmentStatus.Completed,
            endsOn: WorkerScenario.StartsOn.AddMonths(3)
        );

        var freed = Assert.Single(whenFinished.OfType<AssignmentPositionUnstaffed>());
        Assert.Equal(WorkerScenario.PositionId, freed.PositionId);
        Assert.Equal(WorkerScenario.AssignmentId, freed.AssignmentId);
        Assert.Equal(WorkerScenario.ProjectId, freed.ProjectId);
    }

    /// <summary>Not turning up frees the role too - the seat was held and now it is not.</summary>
    [Fact]
    public async Task Handle_SomebodyWhoNeverTurnedUpFreesTheRole()
    {
        var (_, _, messages) = await Handle(WorkerScenario.Planned(), AssignmentStatus.DidNotStart);

        Assert.Single(messages.OfType<AssignmentPositionUnstaffed>());
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

        var (result, _, _) = await Handle(
            WorkerScenario.Planned(),
            AssignmentStatus.Active,
            worker: worker
        );

        Assert.Equal(AssignmentStatus.Active, result.Status);
    }

    private static Task<(
        AssignmentStatusChanged,
        Wolverine.Marten.Events,
        Wolverine.OutgoingMessages
    )> Handle(
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
