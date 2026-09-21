using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Projections;
using HrAgencySystem.Workers.Services;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// Builds a person and their postings at whatever point a test needs, by replaying the events that
/// would have put them there. Nothing reaches into an aggregate's state directly - a test that set
/// up a state the events cannot produce would be testing a situation the system cannot be in.
/// </summary>
internal static class WorkerScenario
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid WorkerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AssignmentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ProjectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid UserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid CompanyId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid LegalEntityId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid PositionId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    public const string PositionName = "Backend developer";

    public static UserSnapshot User { get; } =
        new(UserId, "Alice", "Wells", "alice-wells@hr-agency.com");

    public static PostalAddress Home { get; } =
        PostalAddress.Create("Prosta", "51", null, "00-838", "Warszawa", "PL");

    public static DateOnly Today { get; } = new(2026, 9, 20);
    public static DateOnly StartsOn { get; } = new(2026, 10, 1);

    /// <summary>A Polish national: free movement, so legalisation has nothing to do.</summary>
    public const string PolishCitizenship = "PL";

    /// <summary>A Ukrainian national: needs a residence title and a permit before working.</summary>
    public const string UkrainianCitizenship = "UA";

    public static ProjectSnapshot Project(
        string workCountry = "DE",
        DateOnly? startsOn = null,
        DateOnly? endsOn = null,
        bool open = true
    ) =>
        new(
            ProjectId,
            "Delivery for ACME",
            CompanyId,
            "ACME Corporation",
            LegalEntityId,
            "HR Agency",
            workCountry,
            startsOn ?? new DateOnly(2026, 1, 1),
            endsOn,
            open
        );

    public static PositionSnapshot Position(
        Guid? positionId = null,
        Guid? projectId = null,
        string name = PositionName,
        bool archived = false
    ) => new(positionId ?? PositionId, projectId ?? ProjectId, name, name, archived);

    public static IWorkersService Service(
        ProjectSnapshot? project = null,
        Worker? worker = null,
        PositionSnapshot? position = null
    )
    {
        var service = Substitute.For<IWorkersService>();

        service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(User);
        service
            .GetProjectAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(project ?? Project());
        service
            .GetWorkerAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(worker ?? Registered().Employed());
        service
            .GetPositionAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(position ?? Position());

        return service;
    }

    /// <summary>By default nobody else is on this person's calendar.</summary>
    public static IAssignmentsQueryRepository Assignments(bool overlapping = false)
    {
        var repository = Substitute.For<IAssignmentsQueryRepository>();

        repository
            .HasOverlappingAssignment(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(overlapping);

        repository
            .GetAssignmentsOnPosition(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns([]);

        return repository;
    }

    public static IWorkerEmailReservationRepository EmailReservations(bool taken = false)
    {
        var repository = Substitute.For<IWorkerEmailReservationRepository>();

        repository
            .ExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(taken);

        return repository;
    }

    /// <summary>By default nobody who looks like this person is on the books yet.</summary>
    public static IWorkersQueryRepository Workers(WorkerProjection? duplicate = null)
    {
        var repository = Substitute.For<IWorkersQueryRepository>();

        repository
            .FindDuplicate(
                Arg.Any<OrganizationId>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(duplicate);

        return repository;
    }

    public static WorkerProjection ExistingFile() =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrganizationId,
            FullName = "Jan Kowalski",
        };

    public static IWorkerIdentityDocumentReservationRepository Reservations(bool taken = false)
    {
        var repository = Substitute.For<IWorkerIdentityDocumentReservationRepository>();

        repository
            .ExistsAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(taken);

        return repository;
    }

    public static Worker Registered(
        string citizenship = PolishCitizenship,
        Guid? organizationId = null,
        DateOnly? documentValidUntil = null
    )
    {
        var worker = Worker.Empty();

        worker.Apply(
            new WorkerRegistered(
                WorkerId,
                organizationId ?? OrganizationId,
                "Jan",
                "Kowalski",
                new DateOnly(1990, 5, 12),
                citizenship,
                new IdentityDocument(
                    IdentityDocumentKind.Passport,
                    "ZS1234567",
                    citizenship,
                    documentValidUntil
                ),
                "jan.kowalski@example.com",
                "+48 600 000 000",
                Home,
                "",
                null,
                User,
                DateTimeOffset.UtcNow
            )
        );

        return worker;
    }

    public static Worker InStatus(this Worker worker, WorkerStatus status)
    {
        worker.Apply(
            new WorkerStatusChanged(
                worker.Id.Value,
                worker.OrganizationId.Value,
                worker.Status,
                status,
                "",
                User,
                DateTimeOffset.UtcNow
            )
        );

        return worker;
    }

    /// <summary>Walks a person all the way through the pipeline, skipping what does not apply.</summary>
    public static Worker Employed(this Worker worker)
    {
        worker.InStatus(WorkerStatus.ContractPreparation);

        if (worker.RequiresLegalisation)
            worker.InStatus(WorkerStatus.Legalisation);

        return worker.InStatus(WorkerStatus.Onboarding).InStatus(WorkerStatus.Employed);
    }

    public static Worker WithDocument(this Worker worker, Guid documentId)
    {
        worker.Apply(
            new WorkerDocumentAttached(
                worker.Id.Value,
                worker.OrganizationId.Value,
                new WorkerDocument(
                    documentId,
                    WorkerDocumentCategory.Legalisation,
                    Guid.NewGuid(),
                    "karta-pobytu.pdf",
                    "application/pdf",
                    1024,
                    new DateOnly(2026, 1, 10),
                    null,
                    null
                ),
                User,
                DateTimeOffset.UtcNow
            )
        );

        return worker;
    }

    public static Assignment Planned(
        EngagementType engagement = EngagementType.PostingOfWorkers,
        string workCountry = "DE",
        Guid? organizationId = null,
        DateOnly? endsOn = null
    )
    {
        var assignment = Assignment.Empty();

        assignment.Apply(
            new AssignmentPlanned(
                AssignmentId,
                organizationId ?? OrganizationId,
                WorkerId,
                "Jan Kowalski",
                new ProjectPlacementSnapshot(
                    ProjectId,
                    "Delivery for ACME",
                    CompanyId,
                    "ACME Corporation",
                    LegalEntityId,
                    "HR Agency",
                    workCountry
                ),
                engagement,
                new AssignmentPosition(PositionId, PositionName, PositionName),
                StartsOn,
                endsOn,
                User,
                DateTimeOffset.UtcNow
            )
        );

        return assignment;
    }

    public static Assignment InStatus(
        this Assignment assignment,
        AssignmentStatus status,
        DateOnly? endsOn = null
    )
    {
        assignment.Apply(
            new AssignmentStatusChanged(
                assignment.Id.Value,
                assignment.OrganizationId.Value,
                assignment.WorkerId.Value,
                assignment.Status,
                status,
                endsOn ?? assignment.EndsOn,
                "",
                User,
                DateTimeOffset.UtcNow
            )
        );

        return assignment;
    }

    public static Assignment WithDocument(this Assignment assignment, Guid documentId)
    {
        assignment.Apply(
            new AssignmentDocumentAttached(
                assignment.Id.Value,
                assignment.OrganizationId.Value,
                new AssignmentDocument(
                    documentId,
                    AssignmentDocumentCategory.SocialSecurity,
                    Guid.NewGuid(),
                    "a1.pdf",
                    "application/pdf",
                    2048,
                    new DateOnly(2026, 9, 15),
                    null,
                    null
                ),
                User,
                DateTimeOffset.UtcNow
            )
        );

        return assignment;
    }
}
