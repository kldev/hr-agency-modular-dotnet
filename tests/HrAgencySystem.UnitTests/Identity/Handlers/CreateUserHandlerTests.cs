using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;
using Marten;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class CreateUserHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession = Substitute.For<IDocumentSession>();

    private readonly IIdentityService _service = Substitute.For<IIdentityService>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private readonly IUserEmailReservationRepository _emailReservationRepository =
        Substitute.For<IUserEmailReservationRepository>();

    private static readonly Guid AdminId = Guid.NewGuid();

    private static UserSnapshot Admin { get; } =
        new(AdminId, "Alice", "Wells", "alice-wells@hr-agency.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsUserCreated()
    {
        var organizationId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero);

        const string password = "Password123!";
        const string passwordHash = "hashed-password";

        var organization = new OrganizationInfo(organizationId, "hr-agency", "HR Agency");

        var command = new CreateUser(
            organizationId,
            new ContactPerson(
                "  john.doe@example.com  ",
                "  John  ",
                "  Doe  ",
                "  Senior Recruiter  ",
                "  +48 600 100 200  "
            ),
            OrganizationRole.Admin,
            password,
            Guid.NewGuid()
        );

        _hasher.Hash(password).Returns(passwordHash);

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Admin);

        _service
            .GetOrganization(Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>())
            .Returns(organization);

        _documentSession
            .Events.StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var clock = new FixedClock(now);

        var result = await CreateUserHandler.Handle(
            command,
            _documentSession,
            _hasher,
            _emailReservationRepository,
            _service,
            clock,
            CancellationToken.None
        );

        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal("john.doe@example.com", result.Contact.Email);
        Assert.Equal("John", result.Contact.FirstName);
        Assert.Equal("Doe", result.Contact.LastName);
        Assert.Equal("Senior Recruiter", result.Contact.JobTitle);
        Assert.Equal("+48 600 100 200", result.Contact.Phone);
        Assert.Equal(OrganizationRole.Admin, result.Role);
        Assert.Equal(passwordHash, result.PasswordHash);
        Assert.Equal(organization, result.Organization);
        Assert.Equal(Admin, result.CreatedBy);
        Assert.Equal(now, result.CreatedAt);

        await _service
            .Received(1)
            .ValidateOrganization(organizationId, Arg.Any<CancellationToken>());

        _hasher.Received(1).Hash(password);

        await _emailReservationRepository
            .Received(1)
            .ReserveAsync(
                Arg.Is<OrganizationId>(z => z.Value == organizationId),
                Arg.Is<Email>(z => z.Value == "john.doe@example.com"),
                Arg.Is<UserId>(z => z.Value == result.UserId),
                passwordHash
            );

        var call = _documentSession
            .Events.ReceivedCalls()
            .Single(x => x.GetMethodInfo().Name == nameof(_documentSession.Events.StartStream));

        var arguments = call.GetArguments();

        var @event = Assert.IsType<UserCreated>((arguments[1] as Object[])?[0]);

        Assert.Equal(result.UserId, @event.UserId);
        Assert.Equal(organizationId, @event.OrganizationId);
        Assert.Equal("john.doe@example.com", @event.Contact.Email);
        Assert.Equal("John", @event.Contact.FirstName);
        Assert.Equal("Doe", @event.Contact.LastName);
        Assert.Equal("Senior Recruiter", @event.Contact.JobTitle);
        Assert.Equal("+48 600 100 200", @event.Contact.Phone);
        Assert.Equal(OrganizationRole.Admin, @event.Role);
        Assert.Equal(passwordHash, @event.PasswordHash);
        Assert.Equal(now, @event.CreatedAt);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new CreateUser(
            Guid.NewGuid(),
            new ContactPerson("", "", "", "", ""),
            OrganizationRole.Recruiter,
            "Password123!",
            Guid.NewGuid()
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Equal(
            ["Email is required.", "First name is required.", "Last name is required."],
            exception.Errors
        );

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ThrowsValidationException()
    {
        var command = CommandWith("", "John", "Doe");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Equal(["Email is required."], exception.Errors);

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidFirstName_ThrowsValidationException()
    {
        var command = CommandWith("john.doe@example.com", "", "Doe");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Equal(["First name is required."], exception.Errors);

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidLastName_ThrowsValidationException()
    {
        var command = CommandWith("john.doe@example.com", "John", "");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Equal(["Last name is required."], exception.Errors);

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithEmailTooLong_ThrowsValidationException()
    {
        var command = CommandWith(new string('a', 321) + "@example.com", "John", "Doe");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Contains(exception.Errors, error => error.Contains("Email"));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithFirstNameTooLong_ThrowsValidationException()
    {
        var command = CommandWith("john.doe@example.com", new string('A', 101), "Doe");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Contains(exception.Errors, error => error.Contains("First name"));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithLastNameTooLong_ThrowsValidationException()
    {
        var command = CommandWith("john.doe@example.com", "John", new string('A', 101));

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Contains(exception.Errors, error => error.Contains("Last name"));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithJobTitleTooLong_ThrowsValidationException()
    {
        var command = CommandWith(
            "john.doe@example.com",
            "John",
            "Doe",
            jobTitle: new string('A', 201)
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Contains(exception.Errors, error => error.Contains("Job title"));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithPhoneTooLong_ThrowsValidationException()
    {
        var command = CommandWith(
            "john.doe@example.com",
            "John",
            "Doe",
            phone: new string('1', PersonPhone.MaxLength + 1)
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(() => HandleCommand(command));

        Assert.Contains(exception.Errors, error => error.Contains("Phone"));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsValidationException()
    {
        var command = CommandWith("john.doe@example.com", "John", "Doe", password: "123");

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command)
        );

        Assert.NotEmpty(exception.Message);

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_DoesNotCreateUser()
    {
        var organizationId = Guid.NewGuid();

        var command = CommandWith(
            "john.doe@example.com",
            "John",
            "Doe",
            organizationId: organizationId
        );

        _service
            .ValidateOrganization(organizationId, Arg.Any<CancellationToken>())
            .Throws(new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage));

        await Assert.ThrowsAsync<BusinessRuleException>(() => HandleCommand(command));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());

        _documentSession
            .Events.DidNotReceive()
            .StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToOrganizationChecker()
    {
        var organizationId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        var command = CommandWith(
            "john.doe@example.com",
            "John",
            "Doe",
            organizationId: organizationId
        );

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Admin);

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _service.ValidateOrganization(organizationId, cts.Token).Returns(Task.CompletedTask);

        await HandleCommand(command, ct: cts.Token);

        await _service.Received(1).ValidateOrganization(organizationId, cts.Token);
    }

    [Fact]
    public async Task Handle_ExistEmailInOrganization_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();

        const string password = "Password123!";
        const string passwordHash = "hashed-password";

        var command = new CreateUser(
            organizationId,
            new ContactPerson(
                "  john.doe@example.com  ",
                "  John  ",
                "  Doe  ",
                "Recruiter",
                "+48 600 100 200"
            ),
            OrganizationRole.Admin,
            password,
            Guid.NewGuid()
        );

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Admin);

        _hasher.Hash(password).Returns(passwordHash);

        _documentSession
            .Events.StartStream<User>(Arg.Any<Guid>(), Arg.Any<object>())
            .ReturnsNullForAnyArgs();

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(async () =>
            await HandleCommand(command)
        );

        Assert.Equal(CreateUserHandler.UserWithEmailMessage, exception.Message);
    }

    private static CreateUser CommandWith(
        string email,
        string firstName,
        string lastName,
        string jobTitle = "Recruiter",
        string phone = "+48 600 100 200",
        string password = "Password123!",
        Guid? organizationId = null
    )
    {
        return new CreateUser(
            organizationId ?? Guid.NewGuid(),
            new ContactPerson(email, firstName, lastName, jobTitle, phone),
            OrganizationRole.Recruiter,
            password,
            Guid.NewGuid()
        );
    }

    private async Task HandleCommand(
        CreateUser command,
        IClock? clock = null,
        CancellationToken? ct = null
    )
    {
        await CreateUserHandler.Handle(
            command,
            _documentSession,
            _hasher,
            _emailReservationRepository,
            _service,
            clock ?? new FixedClock(DateTimeOffset.Now),
            ct ?? CancellationToken.None
        );
    }
}
