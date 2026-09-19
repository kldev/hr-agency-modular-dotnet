using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Application.Users.Update;
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
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class UpdateUserHandlerTests : BaseTest
{
    private readonly IIdentityService _service = Substitute.For<IIdentityService>();

    private readonly IUserEmailReservationRepository _emailReservationRepository =
        Substitute.For<IUserEmailReservationRepository>();

    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private static readonly Guid UserGuid = Guid.NewGuid();

    private static UserSnapshot Editor { get; } =
        new(Guid.NewGuid(), "Alice", "Wells", "alice-wells@hr-agency.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsUserUpdated()
    {
        var now = new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero);
        var aggregate = Aggregate();

        var command = Command(
            new ContactPerson(
                "  new.mail@example.com  ",
                "  Johnny  ",
                "  Doe  ",
                "  Lead Recruiter  ",
                "  +48 600 100 200  "
            )
        );

        _service.GetUserAsync(command.ModifiedBy, Arg.Any<CancellationToken>()).Returns(Editor);

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var (@event, events) = await UpdateUserHandler.Handle(
            command,
            aggregate,
            _service,
            _emailReservationRepository,
            new FixedClock(now),
            CancellationToken.None
        );

        Assert.Equal(UserGuid, @event.UserId);
        Assert.Equal(OrganizationGuid, @event.OrganizationId);
        Assert.Equal("new.mail@example.com", @event.Contact.Email);
        Assert.Equal("Johnny", @event.Contact.FirstName);
        Assert.Equal("Doe", @event.Contact.LastName);
        Assert.Equal("Lead Recruiter", @event.Contact.JobTitle);
        Assert.Equal("+48 600 100 200", @event.Contact.Phone);
        Assert.Equal(Editor, @event.ModifiedBy);
        Assert.Equal(now, @event.ModifiedAt);

        Assert.Equal(@event, Assert.Single(events));

        _service.Received(1).ValidateAggregateUpdate(aggregate, OrganizationGuid);
    }

    [Fact]
    public async Task Handle_WhenEmailChanged_MovesTheEmailReservation()
    {
        var aggregate = Aggregate();
        var command = Command(Contact("new.mail@example.com"));

        _service.GetUserAsync(command.ModifiedBy, Arg.Any<CancellationToken>()).Returns(Editor);

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await HandleCommand(command, aggregate);

        await _emailReservationRepository
            .Received(1)
            .ExistAsync(
                Arg.Is<OrganizationId>(z => z.Value == OrganizationGuid),
                Arg.Is<Email>(z => z.Value == "new.mail@example.com"),
                Arg.Any<CancellationToken>()
            );

        await _emailReservationRepository
            .Received(1)
            .ChangeEmailAsync(
                Arg.Is<OrganizationId>(z => z.Value == OrganizationGuid),
                Arg.Is<UserId>(z => z.Value == UserGuid),
                Arg.Is<Email>(z => z.Value == "new.mail@example.com"),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenEmailUnchanged_DoesNotCheckAvailability()
    {
        var aggregate = Aggregate();
        var command = Command(Contact("john.doe@example.com"));

        _service.GetUserAsync(command.ModifiedBy, Arg.Any<CancellationToken>()).Returns(Editor);

        await HandleCommand(command, aggregate);

        await _emailReservationRepository
            .DidNotReceive()
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>());

        await _emailReservationRepository
            .Received(1)
            .ChangeEmailAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenNewEmailIsTaken_ThrowsBusinessRuleException()
    {
        var aggregate = Aggregate();
        var command = Command(Contact("taken@example.com"));

        _service.GetUserAsync(command.ModifiedBy, Arg.Any<CancellationToken>()).Returns(Editor);

        _emailReservationRepository
            .ExistAsync(Arg.Any<OrganizationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command, aggregate)
        );

        Assert.Equal(CreateUserHandler.UserWithEmailMessage, exception.Message);

        await _emailReservationRepository
            .DidNotReceive()
            .ChangeEmailAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithoutModifiedBy_UsesSystemSnapshot()
    {
        var aggregate = Aggregate();
        var command = Command(Contact("john.doe@example.com"), modifiedBy: Guid.Empty);

        var (@event, _) = await UpdateUserHandler.Handle(
            command,
            aggregate,
            _service,
            _emailReservationRepository,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(UserSnapshot.System.Email, @event.ModifiedBy.Email);

        await _service.DidNotReceive().GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidContact_ThrowsValidationExceptionWithAllErrors()
    {
        var aggregate = Aggregate();
        var command = Command(new ContactPerson("", "", "", "", ""));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            HandleCommand(command, aggregate)
        );

        Assert.Equal(
            ["Email is required.", "First name is required.", "Last name is required."],
            exception.Errors
        );

        await _emailReservationRepository
            .DidNotReceive()
            .ChangeEmailAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenAggregateBelongsToAnotherOrganization_DoesNotUpdate()
    {
        var aggregate = Aggregate();
        var command = Command(Contact("new.mail@example.com"));

        _service
            .When(z => z.ValidateAggregateUpdate(aggregate, OrganizationGuid))
            .Throw(new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage));

        await Assert.ThrowsAsync<BusinessRuleException>(() => HandleCommand(command, aggregate));

        await _emailReservationRepository
            .DidNotReceive()
            .ChangeEmailAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<Email>(),
                Arg.Any<CancellationToken>()
            );
    }

    private static ContactPerson Contact(string email) =>
        new(email, "John", "Doe", "Recruiter", "+48 600 100 200");

    private static UpdateUser Command(ContactPerson contact, Guid? modifiedBy = null) =>
        new(UserGuid, OrganizationId.From(OrganizationGuid), contact, modifiedBy ?? Guid.NewGuid());

    private static User Aggregate()
    {
        var user = User.Empty();

        user.Apply(
            new UserCreated(
                UserGuid,
                OrganizationGuid,
                OrganizationRole.Recruiter,
                "hashed-password",
                new OrganizationInfo(OrganizationGuid, "hr-agency", "HR Agency"),
                Editor,
                new ContactPerson(
                    "john.doe@example.com",
                    "John",
                    "Doe",
                    "Recruiter",
                    "+48 600 100 200"
                ),
                DateTimeOffset.UtcNow
            )
        );

        return user;
    }

    private async Task HandleCommand(UpdateUser command, User aggregate)
    {
        await UpdateUserHandler.Handle(
            command,
            aggregate,
            _service,
            _emailReservationRepository,
            TestClock,
            CancellationToken.None
        );
    }
}
