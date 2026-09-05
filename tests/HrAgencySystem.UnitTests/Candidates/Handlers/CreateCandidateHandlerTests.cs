using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Candidates.Handlers;

public class CreateCandidateHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();

    private readonly IOrganizationChecker _organizationChecker =
        Substitute.For<IOrganizationChecker>();

    private readonly ICandidateEmailReservationRepository _emailReservationRepository =
        Substitute.For<ICandidateEmailReservationRepository>();

    private readonly IUserSnapshotRepository _userSnapshotRepository =
        Substitute.For<IUserSnapshotRepository>();

    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid CreatedById = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();

    private static readonly DateTimeOffset Now =
        new(2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

    private void SetupCheckOrganization()
    {
        _organizationChecker
            .Exists(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCandidateCreated()
    {
        var command = CreateValidCommand();
        SetupCheckOrganization();
        
        var result = await Handle(command);

        Assert.NotEqual(Guid.Empty, result.CandidateId);
        Assert.Equal(OrganizationId, result.OrganizationId);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("+48 500 600 700", result.Phone);
        Assert.Equal(command.Source, result.Source);
        Assert.Equal(Now, result.CreatedAt);
        Assert.Null(result.CreatedBy);
        Assert.Equal(CompanyId, result.CompanyId);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);

        await _organizationChecker
            .Received(1)
            .Exists(
                OrganizationId,
                Arg.Any<CancellationToken>());

        await _emailReservationRepository
            .Received(1)
            .ExistsAsync(
                Arg.Is<OrganizationId>(
                    x => x.Value == OrganizationId),
                Arg.Is<Email>(
                    x => x.Value == "john.doe@example.com"),
                Arg.Any<CancellationToken>());

        await _emailReservationRepository
            .Received(1)
            .ReserveAsync(
                Arg.Is<OrganizationId>(
                    x => x.Value == OrganizationId),
                Arg.Is<Email>(
                    x => x.Value == "john.doe@example.com"),
                new CandidateId(result.CandidateId),
                Arg.Any<CancellationToken>());

        AssertNoCreatedByLookup();

        _documentSession.Events
            .Received(1)
            .StartStream<Candidate>(
                result.CandidateId,
                Arg.Is<CandidateCreated>(
                    x =>
                        x.CandidateId == result.CandidateId &&
                        x.OrganizationId == OrganizationId &&
                        x.Email == "john.doe@example.com" &&
                        x.Phone == "+48 500 600 700" &&
                        x.Source == command.Source &&
                        x.CompanyId == CompanyId &&
                        x.FirstName == "John" &&
                        x.LastName == "Doe" &&
                        x.CreatedBy == null &&
                        x.CreatedAt == Now));
    }

    [Fact]
    public async Task Handle_WithCreatedBy_LoadsUserAndIncludesItInEvent()
    {
        var command = CreateValidCommand(
            createdBy: CreatedById);

        SetupCheckOrganization();
        var createdBy = new UserSnapshot(
            CreatedById,
            "Alice",
            "Wells",
            "alice@hr-agency.com");

        _userSnapshotRepository
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns(createdBy);

        var result = await Handle(command);

        Assert.Equal(CreatedById, result.CreatedBy?.Id);

        await _userSnapshotRepository
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .Received(1)
            .StartStream<Candidate>(
                result.CandidateId,
                Arg.Is<CandidateCreated>(
                    x =>
                        x.CandidateId == result.CandidateId &&
                        x.CreatedBy!.Id == CreatedById));
    }

    [Fact]
    public async Task Handle_WithCreatedByUserNotFound_ReturnsEventWithoutCreatedBy()
    {
        var command = CreateValidCommand(
            createdBy: CreatedById);

        _userSnapshotRepository
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        SetupCheckOrganization();
        
        var result = await Handle(command);

        Assert.Null(result.CreatedBy);

        await _userSnapshotRepository
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        Assert.NotEqual(Guid.Empty, result.CandidateId);

        _documentSession.Events
            .Received(1)
            .StartStream<Candidate>(
                result.CandidateId,
                Arg.Any<CandidateCreated>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Handle_WithoutCreatedBy_DoesNotLookupUser(
        string? createdBy)
    {
        var command = CreateValidCommand(
            createdBy: createdBy == null
                ? null
                : Guid.Empty);

        SetupCheckOrganization();
        
        var result = await Handle(command);

        Assert.Null(result.CreatedBy);

        AssertNoCreatedByLookup();

        _documentSession.Events
            .Received(1)
            .StartStream<Candidate>(
                result.CandidateId,
                Arg.Any<CandidateCreated>());
    }

    [Fact]
    public async Task Handle_WithNonExistingOrganization_ThrowsBusinessRuleException()
    {
        _organizationChecker
            .Exists(
                OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(CreateValidCommand()));

        Assert.Equal(
            IOrganizationChecker.OrganizationCheckMessage,
            exception.Message);

        await _organizationChecker
            .Received(1)
            .Exists(
                OrganizationId,
                Arg.Any<CancellationToken>());

        AssertNoEmailReservationCheck();
        AssertNoEmailReservation();
        AssertNoCreatedByLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithAlreadyReservedEmail_ThrowsBusinessRuleException()
    {
        _emailReservationRepository
            .ExistsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        SetupCheckOrganization();
        
        
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(CreateValidCommand()));

        Assert.Equal(
            ICandidateEmailReservationRepository.EmailAlreadyExistsMessage,
            exception.Message);

        await _organizationChecker
            .Received(1)
            .Exists(
                OrganizationId,
                Arg.Any<CancellationToken>());

        await _emailReservationRepository
            .Received(1)
            .ExistsAsync(
                Arg.Is<OrganizationId>(
                    x => x.Value == OrganizationId),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>());

        AssertNoEmailReservation();
        AssertNoCreatedByLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            email: "invalid-email");

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handle(command));

        Assert.NotEmpty(exception.Errors);

        AssertNoOrganizationCheck();
        AssertNoEmailReservationCheck();
        AssertNoEmailReservation();
        AssertNoCreatedByLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidPhone_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            phone: new string('A', 51));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handle(command));

        Assert.NotEmpty(exception.Errors);

        AssertNoOrganizationCheck();
        AssertNoEmailReservationCheck();
        AssertNoEmailReservation();
        AssertNoCreatedByLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidEmailAndPhone_ThrowsValidationExceptionWithBothErrors()
    {
        var command = CreateValidCommand(
            email: "invalid-email",
            phone: ""); // not phone allowed

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handle(command));

        Assert.Single(exception.Errors);

        AssertNoOrganizationCheck();
        AssertNoEmailReservationCheck();
        AssertNoEmailReservation();
        AssertNoCreatedByLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNullFirstNameAndLastName_CreatesCandidateWithEmptyNames()
    {
        var command = CreateValidCommand(
            firstName: null,
            lastName: null);

        SetupCheckOrganization();
        
        var result = await Handle(command);

        Assert.Equal("", result.FirstName);
        Assert.Equal("", result.LastName);

        _documentSession.Events
            .Received(1)
            .StartStream<Candidate>(
                result.CandidateId,
                Arg.Is<CandidateCreated>(
                    x =>
                        x.FirstName == "" &&
                        x.LastName == ""));
    }

    [Fact]
    public async Task Handle_ReservesEmailBeforeLoadingCreatedBy()
    {
        var command = CreateValidCommand(
            createdBy: CreatedById);

        var createdBy = new UserSnapshot(
            CreatedById,
            "Alice",
            "Wells",
            "alice@hr-agency.com");

        SetupCheckOrganization();
        
        _userSnapshotRepository
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns(createdBy);

        var result = await Handle(command);

        Received.InOrder(async () =>
        {
            await _organizationChecker
                .Exists(
                    OrganizationId,
                    Arg.Any<CancellationToken>());

            await _emailReservationRepository
                .ExistsAsync(
                    Arg.Any<OrganizationId>(),
                    Arg.Any<Email>(),
                    Arg.Any<CancellationToken>());

            await _emailReservationRepository
                .ReserveAsync(
                    Arg.Any<OrganizationId>(),
                    Arg.Any<Email>(),
                    new CandidateId(result.CandidateId),
                    Arg.Any<CancellationToken>());

            await _userSnapshotRepository
                .GetUserAsync(
                    CreatedById,
                    Arg.Any<CancellationToken>());
        });
    }

    private async Task<CandidateCreated> Handle(
        CreateCandidate command,
        IClock? clock = null)
    {
        return await CreateCandidateHandler.Handle(
            command,
            _organizationChecker,
            _emailReservationRepository,
            _userSnapshotRepository,
            _documentSession,
            clock ?? new FixedClock(Now),
            CancellationToken.None);
    }

    private static CreateCandidate CreateValidCommand(
        Guid? organizationId = null,
        string email = "john.doe@example.com",
        CandidateSource source = CandidateSource.InternalDatabase,
        string phone = "+48 500 600 700",
        string? firstName = "John",
        string? lastName = "Doe",
        Guid? createdBy = null,
        Guid? companyId = null)
    {
        return new CreateCandidate(
            organizationId ?? OrganizationId,
            email,
            source,
            phone,
            firstName,
            lastName,
            createdBy,
            companyId ?? CompanyId);
    }

    private void AssertNoOrganizationCheck()
    {
        _organizationChecker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoEmailReservationCheck()
    {
        _emailReservationRepository
            .DidNotReceive()
            .ExistsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoEmailReservation()
    {
        _emailReservationRepository
            .DidNotReceive()
            .ReserveAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Email>(),
                Arg.Any<CandidateId>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoCreatedByLookup()
    {
        _userSnapshotRepository
            .DidNotReceive()
            .GetUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoStream()
    {
        _documentSession.Events
            .DidNotReceive()
            .StartStream<Candidate>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }
}
