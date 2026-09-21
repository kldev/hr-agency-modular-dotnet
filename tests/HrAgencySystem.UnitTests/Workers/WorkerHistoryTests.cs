using HrAgencySystem.Compliance;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Projections;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// A person's employment history is not a thing anybody maintains - it is the list of their
/// assignments, which is why the two are separate aggregates. These tests drive the projector the
/// way the daemon does and check that moving somebody leaves the record of where they were.
/// </summary>
public class WorkerHistoryTests
{
    private readonly WorkerProjector _projector = new();

    [Fact]
    public void MovingToAnotherProjectLeavesTheFirstPostingUntouched()
    {
        var worker = _projector.Create(Registered());

        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        _projector.Apply(worker, Planned(first, "Warsaw delivery", "PL", new DateOnly(2026, 1, 1)));
        _projector.Apply(
            worker,
            Ended(first, AssignmentStatus.Completed, new DateOnly(2026, 3, 31))
        );
        _projector.Apply(
            worker,
            Planned(second, "Berlin delivery", "DE", new DateOnly(2026, 4, 1))
        );

        Assert.Equal(2, worker.Assignments.Count);

        // The first one still says what actually happened: the Polish project, its own dates, and
        // finished. Nothing was repointed.
        var completed = worker.Assignments.Single(a => a.AssignmentId == first);
        Assert.Equal("Warsaw delivery", completed.ProjectName);
        Assert.Equal("PL", completed.WorkCountry);
        Assert.Equal(new DateOnly(2026, 1, 1), completed.StartsOn);
        Assert.Equal(new DateOnly(2026, 3, 31), completed.EndsOn);
        Assert.Equal(AssignmentStatus.Completed, completed.Status);
    }

    /// <summary>
    /// The column the two halves of the office filter on. Somebody working in Poland belongs to one
    /// list and somebody posted abroad to the other, and the answer moves when they do.
    /// </summary>
    [Fact]
    public void TheCurrentWorkCountryFollowsThePersonAbroad()
    {
        var worker = _projector.Create(Registered());
        var polish = Guid.NewGuid();

        _projector.Apply(
            worker,
            Planned(polish, "Warsaw delivery", "PL", new DateOnly(2026, 1, 1))
        );
        Assert.Equal("PL", worker.CurrentWorkCountry);

        _projector.Apply(
            worker,
            Ended(polish, AssignmentStatus.Completed, new DateOnly(2026, 3, 31))
        );
        _projector.Apply(
            worker,
            Planned(Guid.NewGuid(), "Berlin delivery", "DE", new DateOnly(2026, 4, 1))
        );

        Assert.Equal("DE", worker.CurrentWorkCountry);
        Assert.Equal("Berlin delivery", worker.CurrentProjectName);
    }

    [Fact]
    public void SomebodyWithNoPostingHasNoWorkCountryAtAll()
    {
        var worker = _projector.Create(Registered());

        Assert.Null(worker.CurrentWorkCountry);
        Assert.Equal(0, worker.AssignmentCount);
    }

    [Fact]
    public void APostingThatNeverStartedStopsCountingAsOpen()
    {
        var worker = _projector.Create(Registered());
        var assignment = Guid.NewGuid();

        _projector.Apply(
            worker,
            Planned(assignment, "Berlin delivery", "DE", new DateOnly(2026, 4, 1))
        );
        Assert.Equal(1, worker.OpenAssignmentCount);

        _projector.Apply(worker, Ended(assignment, AssignmentStatus.DidNotStart, null));

        // The days are free again, which is what lets somebody else be planned into them.
        Assert.Equal(0, worker.OpenAssignmentCount);
        Assert.Equal(1, worker.AssignmentCount);
    }

    [Fact]
    public void TheStageAndTheDepartmentMoveTogether()
    {
        var worker = _projector.Create(Registered());

        _projector.Apply(
            worker,
            new WorkerStatusChanged(
                WorkerScenario.WorkerId,
                WorkerScenario.OrganizationId,
                WorkerStatus.Recruitment,
                WorkerStatus.ContractPreparation,
                "",
                WorkerScenario.User,
                DateTimeOffset.UtcNow
            )
        );

        Assert.Equal(WorkerStatus.ContractPreparation, worker.Status);
        Assert.Equal(ResponsibleDepartment.HumanResources, worker.Department);
    }

    private static WorkerRegistered Registered() =>
        new(
            WorkerScenario.WorkerId,
            WorkerScenario.OrganizationId,
            "Jan",
            "Kowalski",
            new DateOnly(1990, 5, 12),
            WorkerScenario.PolishCitizenship,
            new IdentityDocument(
                IdentityDocumentKind.IdentityCard,
                "ABC123456",
                "PL",
                new DateOnly(2030, 1, 1)
            ),
            null,
            "+48 600 000 000",
            WorkerScenario.Home,
            "",
            null,
            WorkerScenario.User,
            DateTimeOffset.UtcNow
        );

    private static AssignmentPlanned Planned(
        Guid assignmentId,
        string projectName,
        string workCountry,
        DateOnly startsOn
    ) =>
        new(
            assignmentId,
            WorkerScenario.OrganizationId,
            WorkerScenario.WorkerId,
            "Jan Kowalski",
            new ProjectPlacementSnapshot(
                Guid.NewGuid(),
                projectName,
                WorkerScenario.CompanyId,
                "ACME Corporation",
                WorkerScenario.LegalEntityId,
                "HR Agency",
                workCountry
            ),
            EngagementType.PostingOfWorkers,
            new AssignmentPosition(
                WorkerScenario.PositionId,
                WorkerScenario.PositionName,
                WorkerScenario.PositionName
            ),
            startsOn,
            null,
            WorkerScenario.User,
            DateTimeOffset.UtcNow
        );

    private static AssignmentStatusChanged Ended(
        Guid assignmentId,
        AssignmentStatus status,
        DateOnly? endsOn
    ) =>
        new(
            assignmentId,
            WorkerScenario.OrganizationId,
            WorkerScenario.WorkerId,
            AssignmentStatus.Planned,
            status,
            endsOn,
            "",
            WorkerScenario.User,
            DateTimeOffset.UtcNow
        );
}
