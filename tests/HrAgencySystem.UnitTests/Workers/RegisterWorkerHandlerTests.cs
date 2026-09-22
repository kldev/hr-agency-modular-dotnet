using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Application.RegisterWorker;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Workers;

public class RegisterWorkerHandlerTests : BaseTest
{
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsWorkerRegistered()
    {
        var result = await Handle(Command());

        Assert.NotEqual(Guid.Empty, result.WorkerId);
        Assert.Equal(WorkerScenario.OrganizationId, result.OrganizationId);
        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("Kowalski", result.LastName);
        Assert.Equal("UA", result.Citizenship);

        _session
            .Events.Received(1)
            .StartStream<Worker>(
                result.WorkerId,
                Arg.Is<WorkerRegistered>(e => e.WorkerId == result.WorkerId)
            );
    }

    /// <summary>
    /// Everybody enters at the same door. A file that appeared already employed would have skipped
    /// recruitment's desk and HR's, and nobody would know which papers were never collected.
    /// </summary>
    [Fact]
    public async Task Handle_StartsThePersonInRecruitment()
    {
        var result = await Handle(Command());

        var worker = Worker.Empty();
        worker.Apply(result);

        Assert.Equal(WorkerStatus.Recruitment, worker.Status);
        Assert.Equal(
            ResponsibleDepartment.Recruitment,
            WorkerStatusChangePolicy.OwnerOf(worker.Status)
        );
    }

    [Fact]
    public async Task Handle_ReservesTheIdentityDocument()
    {
        var reservations = WorkerScenario.Reservations();

        var result = await Handle(Command(), reservations: reservations);

        await reservations
            .Received(1)
            .ReserveAsync(WorkerScenario.OrganizationId, result.WorkerId, "UA", "ZS1234567");
    }

    [Fact]
    public async Task Handle_WhenTheDocumentIsAlreadyOnFile_Refuses()
    {
        // One person, one file. Two files for the same passport would make the A1 register count
        // the same human twice and still leave one of them uncovered.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), reservations: WorkerScenario.Reservations(taken: true))
        );

        Assert.Equal(
            IWorkerIdentityDocumentReservationRepository.AlreadyUsedMessage,
            error.Message
        );
    }

    [Fact]
    public async Task Handle_NormalizesTheDocumentNumberAndCountries()
    {
        var result = await Handle(
            Command() with
            {
                IdentityDocumentNumber = "  zs1234567 ",
                IdentityDocumentIssuingCountry = "ua",
                Citizenship = "ua",
            }
        );

        Assert.Equal("ZS1234567", result.IdentityDocument.Number);
        Assert.Equal("UA", result.IdentityDocument.IssuingCountry);
        Assert.Equal("UA", result.Citizenship);
    }

    [Fact]
    public async Task Handle_WithoutAnAddress_IsAllowed()
    {
        // A person can be on file before anybody has their address.
        var result = await Handle(
            Command() with
            {
                Street = null,
                BuildingNumber = null,
                PostalCode = null,
                City = null,
                AddressCountryCode = null,
            }
        );

        Assert.Null(result.Address);
    }

    [Fact]
    public async Task Handle_WithHalfAnAddress_ThrowsValidation()
    {
        // Half an address on a posting declaration is worse than none.
        await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { City = null, PostalCode = null })
        );
    }

    [Fact]
    public async Task Handle_WithABirthDateInTheFuture_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { DateOfBirth = WorkerScenario.Today.AddDays(1) })
        );

        Assert.Contains(WorkerDataFactory.DateOfBirthInFutureMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_CollectsEveryProblemAtOnce()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                Command() with
                {
                    FirstName = "",
                    LastName = "",
                    Citizenship = "PLX",
                    IdentityDocumentNumber = "",
                    Email = "not-an-email",
                }
            )
        );

        Assert.Contains(FirstName.RequiredMessage, error.Errors);
        Assert.Contains(LastName.RequiredMessage, error.Errors);
        Assert.Contains(CountryCode.InvalidFormatMessage, error.Errors);
        Assert.Equal(5, error.Errors.Count);
    }

    [Fact]
    public async Task Handle_RecordsWhereTheFileCameFromWithoutLinkingToIt()
    {
        var candidateId = Guid.NewGuid();

        var result = await Handle(Command() with { SourceCandidateId = candidateId });

        Assert.Equal(candidateId, result.SourceCandidateId);
    }

    /// <summary>Recruitment is told, so the candidate and the application can say so too.</summary>
    [Fact]
    public async Task Handle_FromAnApplication_TellsRecruitment()
    {
        var candidateId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();

        var (registered, message) = await HandleWithMessages(
            Command() with { SourceCandidateId = candidateId, SourceApplicationId = applicationId }
        );

        Assert.Equal(applicationId, registered.SourceApplicationId);
        Assert.NotNull(message);
        Assert.Equal(registered.WorkerId, message.WorkerId);
        Assert.Equal(WorkerScenario.OrganizationId, message.OrganizationId);
        Assert.Equal(candidateId, message.CandidateId);
        Assert.Equal(applicationId, message.ApplicationId);
    }

    [Fact]
    public async Task Handle_FromACandidateAlone_TellsRecruitmentWithoutAnApplication()
    {
        var candidateId = Guid.NewGuid();

        var (_, message) = await HandleWithMessages(Command() with { SourceCandidateId = candidateId });

        Assert.NotNull(message);
        Assert.Null(message.ApplicationId);
    }

    /// <summary>Somebody who did not come through recruitment is nobody's news there.</summary>
    [Fact]
    public async Task Handle_WithoutASource_TellsNobody()
    {
        var (_, message) = await HandleWithMessages(Command());

        Assert.Null(message);
    }

    [Fact]
    public async Task Handle_WithAnApplicationButNoCandidate_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(Command() with { SourceApplicationId = Guid.NewGuid() })
        );

        Assert.Contains(RegisterWorkerHandler.ApplicationWithoutCandidateMessage, error.Message);
    }

    private async Task<WorkerRegistered> Handle(
        RegisterWorker command,
        IWorkerIdentityDocumentReservationRepository? reservations = null,
        IWorkerEmailReservationRepository? emails = null,
        IWorkersQueryRepository? workers = null
    ) => (await HandleWithMessages(command, reservations, emails, workers)).Item1;

    private Task<(WorkerRegistered, WorkerRegisteredFromRecruitment?)> HandleWithMessages(
        RegisterWorker command,
        IWorkerIdentityDocumentReservationRepository? reservations = null,
        IWorkerEmailReservationRepository? emails = null,
        IWorkersQueryRepository? workers = null
    ) =>
        RegisterWorkerHandler.Handle(
            command,
            WorkerScenario.Service(),
            reservations ?? WorkerScenario.Reservations(),
            emails ?? WorkerScenario.EmailReservations(),
            workers ?? WorkerScenario.Workers(),
            _session,
            new FixedClock(
                new DateTimeOffset(
                    WorkerScenario.Today.ToDateTime(TimeOnly.MinValue),
                    TimeSpan.Zero
                )
            ),
            CancellationToken.None
        );

    /// <summary>
    /// One person, one file. The rule matters more than it looks: a second file splits somebody's
    /// postings in half, and then neither half can be asked whether they hold a valid A1.
    /// </summary>
    [Fact]
    public async Task Handle_WhenTheEmailIsAlreadyOnFile_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), emails: WorkerScenario.EmailReservations(taken: true))
        );

        Assert.Equal(IWorkerEmailReservationRepository.AlreadyUsedMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WhenSomebodyWithThatNameAndNumberIsOnFile_Refuses()
    {
        // The check for everybody who has no work address. The answer says what to do instead.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(Command(), workers: WorkerScenario.Workers(WorkerScenario.ExistingFile()))
        );

        Assert.Equal(RegisterWorkerHandler.AlreadyOnFileMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ReservesTheEmailWhenThereIsOne()
    {
        var emails = WorkerScenario.EmailReservations();

        var result = await Handle(Command(), emails: emails);

        await emails
            .Received(1)
            .ReserveAsync(
                WorkerScenario.OrganizationId,
                result.WorkerId,
                "jan.kowalski@example.com"
            );
    }

    [Fact]
    public async Task Handle_WithoutAnEmail_ReservesNothing()
    {
        var emails = WorkerScenario.EmailReservations();

        await Handle(Command() with { Email = null }, emails: emails);

        await emails
            .DidNotReceive()
            .ReserveAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>());
    }

    private static RegisterWorker Command() =>
        new(
            WorkerScenario.OrganizationId,
            "Jan",
            "Kowalski",
            new DateOnly(1990, 5, 12),
            WorkerScenario.UkrainianCitizenship,
            IdentityDocumentKind.Passport,
            "ZS1234567",
            "UA",
            new DateOnly(2030, 1, 1),
            "jan.kowalski@example.com",
            "+48 600 000 000",
            "Prosta",
            "51",
            null,
            "00-838",
            "Warszawa",
            "PL",
            null,
            null,
            null,
            WorkerScenario.UserId
        );
}
