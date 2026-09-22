using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Integration;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Candidates;

/// <summary>
/// The ids arrive from a module that cannot see them, so every case here is a claim the handler
/// has to check - and a claim that does not hold is skipped, never thrown.
/// </summary>
public class WorkerRegisteredFromRecruitmentHandlerTests : BaseTest
{
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid CandidateId = Guid.NewGuid();
    private static readonly Guid ApplicationId = Guid.NewGuid();
    private static readonly Guid WorkerId = Guid.NewGuid();

    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();

    public WorkerRegisteredFromRecruitmentHandlerTests()
    {
        _session.Events.Returns(_events);
    }

    [Fact]
    public async Task Handle_MarksTheCandidateAndTheApplication()
    {
        Given(Candidate(), Application());

        await Handle(Message());

        CandidateMarked();
        ApplicationMarked();
    }

    [Fact]
    public async Task Handle_WithoutAnApplication_MarksOnlyTheCandidate()
    {
        Given(Candidate(), null);

        await Handle(Message() with { ApplicationId = null });

        CandidateMarked();
        ApplicationNotMarked();
    }

    /// <summary>Another organization's candidate does not get this organization's worker.</summary>
    [Fact]
    public async Task Handle_ACandidateOfAnotherOrganization_IsSkipped()
    {
        Given(Candidate(organizationId: Guid.NewGuid()), Application());

        await Handle(Message());

        CandidateNotMarked();
        ApplicationNotMarked();
    }

    [Fact]
    public async Task Handle_AnUnknownCandidate_IsSkipped()
    {
        Given(null, Application());

        await Handle(Message());

        CandidateNotMarked();
        ApplicationNotMarked();
    }

    /// <summary>
    /// The pair does not belong together. The person is still in the register, so the candidate
    /// says so; the application of somebody else does not claim the file.
    /// </summary>
    [Fact]
    public async Task Handle_AnApplicationOfAnotherCandidate_LeavesTheApplicationAlone()
    {
        Given(Candidate(), Application(candidateId: Guid.NewGuid()));

        await Handle(Message());

        CandidateMarked();
        ApplicationNotMarked();
    }

    [Fact]
    public async Task Handle_AnApplicationOfAnotherOrganization_LeavesTheApplicationAlone()
    {
        Given(Candidate(), Application(organizationId: Guid.NewGuid()));

        await Handle(Message());

        CandidateMarked();
        ApplicationNotMarked();
    }

    /// <summary>These messages arrive at least once; the second one changes nothing.</summary>
    [Fact]
    public async Task Handle_ARepeat_RecordsNothingTwice()
    {
        Given(Candidate(workerId: WorkerId), Application(workerId: WorkerId));

        await Handle(Message());

        CandidateNotMarked();
        ApplicationNotMarked();
    }

    /// <summary>One person, one file: the file the history already hangs on stays.</summary>
    [Fact]
    public async Task Handle_ACandidateWithAnotherWorker_KeepsTheFirst()
    {
        Given(Candidate(workerId: Guid.NewGuid()), null);

        await Handle(Message() with { ApplicationId = null });

        CandidateNotMarked();
    }

    private static WorkerRegisteredFromRecruitment Message() =>
        new(OrganizationId, WorkerId, CandidateId, ApplicationId);

    private Task Handle(WorkerRegisteredFromRecruitment message) =>
        WorkerRegisteredFromRecruitmentHandler.Handle(
            message,
            _session,
            TestClock,
            NullLogger<WorkerRegisteredFromRecruitment>.Instance,
            CancellationToken.None
        );

    private void Given(Candidate? candidate, JobApplication? application)
    {
        _events
            .AggregateStreamAsync<Candidate>(
                CandidateId,
                Arg.Any<long>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<Candidate?>(),
                Arg.Any<long>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(candidate);

        _events
            .AggregateStreamAsync<JobApplication>(
                ApplicationId,
                Arg.Any<long>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<JobApplication?>(),
                Arg.Any<long>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(application);
    }

    private void CandidateMarked() =>
        _events
            .Received(1)
            .Append(
                CandidateId,
                Arg.Is<object[]>(events =>
                    events.Length == 1
                    && events[0] is CandidateRegisteredAsWorker
                    && ((CandidateRegisteredAsWorker)events[0]).WorkerId == WorkerId
                )
            );

    private void CandidateNotMarked() =>
        _events.DidNotReceive().Append(CandidateId, Arg.Any<object[]>());

    private void ApplicationMarked() =>
        _events
            .Received(1)
            .Append(
                ApplicationId,
                Arg.Is<object[]>(events =>
                    events.Length == 1
                    && events[0] is JobApplicationRegisteredAsWorker
                    && ((JobApplicationRegisteredAsWorker)events[0]).WorkerId == WorkerId
                )
            );

    private void ApplicationNotMarked() =>
        _events.DidNotReceive().Append(ApplicationId, Arg.Any<object[]>());

    private static Candidate Candidate(Guid? organizationId = null, Guid? workerId = null)
    {
        var candidate = Recruitment.Domain.Candidates.Candidate.Empty();

        candidate.Apply(
            new CandidateCreated(
                CandidateId,
                organizationId ?? OrganizationId,
                "anna.kowalska@example.com",
                "+48 600 000 000",
                CandidateSource.Direct,
                DateTimeOffset.UtcNow,
                null,
                null,
                "Anna",
                "Kowalska"
            )
        );

        if (workerId is { } id)
            candidate.Apply(new CandidateRegisteredAsWorker(CandidateId, id, DateTimeOffset.UtcNow));

        return candidate;
    }

    private static JobApplication Application(
        Guid? organizationId = null,
        Guid? candidateId = null,
        Guid? workerId = null
    )
    {
        var application = JobApplication.Empty();

        application.Apply(
            new JobApplicationCreated(
                ApplicationId,
                organizationId ?? OrganizationId,
                Guid.NewGuid(),
                "Backend developer",
                CandidateSource.Direct,
                new CompanySnapshot(Guid.NewGuid(), "", ""),
                new CandidateInfo(
                    candidateId ?? CandidateId,
                    "anna.kowalska@example.com",
                    "+48 600 000 000",
                    "Anna",
                    "Kowalska"
                ),
                "anna.kowalska@example.com",
                "+48 600 000 000",
                "Anna",
                "Kowalska",
                DateTimeOffset.UtcNow
            )
        );

        if (workerId is { } id)
            application.Apply(
                new JobApplicationRegisteredAsWorker(ApplicationId, id, DateTimeOffset.UtcNow)
            );

        return application;
    }
}
