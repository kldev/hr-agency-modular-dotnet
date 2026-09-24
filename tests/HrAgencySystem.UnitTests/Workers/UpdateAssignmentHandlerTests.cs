using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.UpdateAssignment;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Wolverine;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// Correcting a posting. The role is an id now, so the interesting cases are the ones where that
/// id is wrong or moves - whether moving it should be a correction at all is the open question on
/// the plan, but while it is one the counts may not be left behind.
/// </summary>
public class UpdateAssignmentHandlerTests : BaseTest
{
    [Fact]
    public async Task Handle_CorrectingThePeriodLeavesTheRoleWhereItWas()
    {
        var assignment = WorkerScenario.Planned();

        var (updated, _, messages) = await Handle(assignment, Command());

        Assert.Equal(WorkerScenario.PositionId, updated.Position.PositionId);
        Assert.Empty(messages);
    }

    [Fact]
    public async Task Handle_CorrectingTheRoleMovesTheSeatRatherThanAddingOne()
    {
        var assignment = WorkerScenario.Planned();
        var corrected = Guid.NewGuid();

        var (_, _, messages) = await Handle(
            assignment,
            Command() with
            {
                PositionId = corrected,
            },
            position: WorkerScenario.Position(positionId: corrected, name: "Bricklayer")
        );

        var freed = Assert.Single(messages.OfType<AssignmentPositionUnstaffed>());
        var taken = Assert.Single(messages.OfType<AssignmentPositionStaffed>());

        Assert.Equal(WorkerScenario.PositionId, freed.PositionId);
        Assert.Equal(corrected, taken.PositionId);
        Assert.Equal(WorkerScenario.AssignmentId, taken.AssignmentId);
    }

    private static Task<(AssignmentUpdated, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        Assignment assignment,
        UpdateAssignment command,
        PositionSnapshot? position = null
    ) =>
        UpdateAssignmentHandler.Handle(
            command,
            assignment,
            WorkerScenario.Service(position: position),
            WorkerScenario.Assignments(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static UpdateAssignment Command() =>
        new(
            WorkerScenario.AssignmentId,
            WorkerScenario.OrganizationId,
            WorkerScenario.PositionId,
            WorkerScenario.StartsOn,
            WorkerScenario.StartsOn.AddMonths(6),
            WorkerScenario.UserId
        );
}
