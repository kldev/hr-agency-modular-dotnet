using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Handlers;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class CreateUserHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();

    private readonly IOrganizationChecker _checker =
        Substitute.For<IOrganizationChecker>();

    private readonly IPasswordHasher _hasher =
        Substitute.For<IPasswordHasher>();

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
            password);

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        _hasher
            .Hash(password)
            .Returns(passwordHash);

        _documentSession
            .Events
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        var clock = new FixedClock(now);

        var result = await CreateUserHandler.Handle(
            command,
            _documentSession,
            clock,
            _checker,
            _hasher,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal(OrganizationRole.Admin, result.Role);
        Assert.Equal(passwordHash, result.PasswordHash);
        Assert.Equal(now, result.CreatedAt);

        await _checker
            .Received(1)
            .Exists(
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
            "Password123!"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!");

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "123"
            );

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);
        
        

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!"
            );

        _checker
            .Exists(
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                CancellationToken.None));

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
            "Password123!");

        _checker
            .Exists(
                organizationId,
                cts.Token)
            .Returns(false);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateUserHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _hasher,
                cts.Token));

        await _checker
            .Received(1)
            .Exists(
                organizationId,
                cts.Token);
    }
}
