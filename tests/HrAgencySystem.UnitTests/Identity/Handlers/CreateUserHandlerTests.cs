using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class CreateUserHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();

    private readonly IIdentityService _service =
        Substitute.For<IIdentityService>();

    private readonly IPasswordHasher _hasher =
        Substitute.For<IPasswordHasher>();

    private readonly IUserEmailReservationRepository _emailReservationRepository
        = Substitute.For<IUserEmailReservationRepository>();
    
    private static readonly Guid AdminId = Guid.NewGuid();

    private static UserSnapshot Admin { get; } =
        new(
            AdminId,
            "Alice",
            "Wells",
            "alice-wells@hr-agency.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsUserCreated()
    {
        var organizationId = Guid.NewGuid();
        var now = new DateTimeOffset(
            2026, 8, 31, 10, 0, 0, TimeSpan.Zero);

        const string password = "Password123!";
        const string passwordHash = "hashed-password";

        var command = new CreateUser(
            organizationId,
            "  john.doe@example.com  ",
            "  John  ",
            "  Doe  ",
            OrganizationRole.Admin,
            password, Guid.NewGuid());

        _hasher
            .Hash(password)
            .Returns(passwordHash);
        
        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Admin);

        _documentSession
            .Events
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        _emailReservationRepository.ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);
        
        var clock = new FixedClock(now);

        var result = await CreateUserHandler.Handle(
            command,
            _documentSession,
            _hasher,
            _emailReservationRepository,
            _service,
            clock,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal(OrganizationRole.Admin, result.Role);
        Assert.Equal(passwordHash, result.PasswordHash);
        Assert.Equal(now, result.CreatedAt);

        await _service
            .Received(1)
            .ValidateOrganization(
                organizationId,
                Arg.Any<CancellationToken>());

        _hasher
            .Received(1)
            .Hash(password);

        var call = _documentSession.Events
            .ReceivedCalls()
            .Single(x => x.GetMethodInfo().Name == nameof(_documentSession.Events.StartStream));

        var arguments = call.GetArguments();

        var @event = Assert.IsType<UserCreated>((arguments[1] as Object[])?[0]);

        Assert.Equal(result.UserId, @event.UserId);
        Assert.Equal(organizationId, @event.OrganizationId);
        Assert.Equal("john.doe@example.com", @event.Email);
        Assert.Equal("John", @event.FirstName);
        Assert.Equal("Doe", @event.LastName);
        Assert.Equal(OrganizationRole.Admin, @event.Role);
        Assert.Equal(passwordHash, @event.PasswordHash);
        Assert.Equal(now, @event.CreatedAt);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "",
            "",
            "",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Equal(
            [
                "Email is required.",
                "First name is required.",
                "Last name is required."
            ],
            exception.Errors);

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "",
            "John",
            "Doe",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Equal(
            ["Email is required."],
            exception.Errors);

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidFirstName_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "john.doe@example.com",
            "",
            "Doe",
            OrganizationRole.Recruiter,
            "Password123!",Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Equal(
            ["First name is required."],
            exception.Errors);

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidLastName_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "john.doe@example.com",
            "John",
            "",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Equal(
            ["Last name is required."],
            exception.Errors);

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithEmailTooLong_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            new string('a', 321) + "@example.com",
            "John",
            "Doe",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Contains(
            exception.Errors,
            error => error.Contains("Email"));

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithFirstNameTooLong_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "john.doe@example.com",
            new string('A', 101),
            "Doe",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Contains(
            exception.Errors,
            error => error.Contains("First name"));

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithLastNameTooLong_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "john.doe@example.com",
            "John",
            new string('A', 101),
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command));

        Assert.Contains(
            exception.Errors,
            error => error.Contains("Last name"));

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsValidationException()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            "john.doe@example.com",
            "John",
            "Doe",
            OrganizationRole.Recruiter,
            "123", Guid.NewGuid()
            );

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command));

        Assert.NotEmpty(exception.Message);

        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }
    
    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_DoesNotCreateUser()
    {
        var organizationId = Guid.NewGuid();

        var command = new CreateUser(
            organizationId,
            "john.doe@example.com",
            "John",
            "Doe",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid()
            );

        _service.ValidateOrganization(organizationId, Arg.Any<CancellationToken>())
            .Throws(new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command));


        _hasher
            .DidNotReceive()
            .Hash(Arg.Any<string>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<User>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToOrganizationChecker()
    {
        var organizationId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        var command = new CreateUser(
            organizationId,
            "john.doe@example.com",
            "John",
            "Doe",
            OrganizationRole.Recruiter,
            "Password123!", Guid.NewGuid());


        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);

        _service.ValidateOrganization(organizationId, cts.Token).Returns(Task.CompletedTask);

        await HandleCommand(command, ct: cts.Token);
        
        await _service
            .Received(1)
            .ValidateOrganization(
                organizationId,
                cts.Token);
    }

    [Fact]
    public async Task Handle_ExistEmailInOrganization_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();
        var now = new DateTimeOffset(
            2026, 8, 31, 10, 0, 0, TimeSpan.Zero);

        const string password = "Password123!";
        const string passwordHash = "hashed-password";

        var command = new CreateUser(
            organizationId,
            "  john.doe@example.com  ",
            "  John  ",
            "  Doe  ",
            OrganizationRole.Admin,
            password, Guid.NewGuid());
        
        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Admin);

        _hasher
            .Hash(password)
            .Returns(passwordHash);

        _documentSession
            .Events
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>( async () => await HandleCommand(command));
        
        Assert.Equal(CreateUserHandler.UserWithEmailMessage, exception.Message);
    }

    private async Task HandleCommand(CreateUser command, IClock? clock = null, CancellationToken? ct = null)
    {
        await CreateUserHandler.Handle(
            command,
            _documentSession,
            _hasher,
            _emailReservationRepository,
            _service,
            clock ?? new FixedClock(DateTimeOffset.Now),
            ct ?? CancellationToken.None);
    }
}
