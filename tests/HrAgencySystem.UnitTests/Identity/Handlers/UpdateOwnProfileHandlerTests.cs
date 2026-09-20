using HrAgencySystem.Identity.Application.Users.UpdateOwnProfile;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.Web.Common;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class UpdateOwnProfileHandlerTests : BaseTest
{
    private readonly IIdentityService _service = Substitute.For<IIdentityService>();

    private const string AggregateEmail = "john.doe@example.com";

    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private static readonly Guid UserGuid = Guid.NewGuid();

    private static UserSnapshot Self { get; } = new(UserGuid, "John", "Doe", AggregateEmail);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsUserUpdated()
    {
        var now = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);
        var aggregate = Aggregate();

        _service.GetUserAsync(UserGuid, Arg.Any<CancellationToken>()).Returns(Self);

        var (@event, events) = await UpdateOwnProfileHandler.Handle(
            Command(),
            aggregate,
            _service,
            new FixedClock(now),
            CancellationToken.None
        );

        Assert.Equal(UserGuid, @event.UserId);
        Assert.Equal(OrganizationGuid, @event.OrganizationId);
        Assert.Equal("Johnny", @event.Contact.FirstName);
        Assert.Equal("Doe", @event.Contact.LastName);
        Assert.Equal("Lead Recruiter", @event.Contact.JobTitle);
        Assert.Equal("+48 600 100 200", @event.Contact.Phone);
        Assert.Equal(Self, @event.ModifiedBy);
        Assert.Equal(now, @event.ModifiedAt);

        Assert.Equal(@event, Assert.Single(events));

        _service.Received(1).ValidateAggregateUpdate(aggregate, OrganizationGuid);
    }

    /// <summary>
    /// The whole point of the separate command: the address on the event is the aggregate's own, and
    /// there is no field on the command that could ever say otherwise.
    /// </summary>
    [Fact]
    public async Task Handle_KeepsTheAddressTheAggregateAlreadyHas()
    {
        var aggregate = Aggregate();

        _service.GetUserAsync(UserGuid, Arg.Any<CancellationToken>()).Returns(Self);

        var (@event, _) = await UpdateOwnProfileHandler.Handle(
            Command(),
            aggregate,
            _service,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(AggregateEmail, @event.Contact.Email);
    }

    [Fact]
    public async Task Handle_TrimsWhatThePersonTyped()
    {
        var aggregate = Aggregate();

        _service.GetUserAsync(UserGuid, Arg.Any<CancellationToken>()).Returns(Self);

        var (@event, _) = await UpdateOwnProfileHandler.Handle(
            Command("  Johnny  ", "  Doe  "),
            aggregate,
            _service,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal("Johnny", @event.Contact.FirstName);
        Assert.Equal("Doe", @event.Contact.LastName);
    }

    [Fact]
    public async Task Handle_WithBlankNames_Throws()
    {
        var aggregate = Aggregate();

        await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateOwnProfileHandler.Handle(
                Command("", ""),
                aggregate,
                _service,
                TestClock,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Handle_FromAnotherOrganization_Throws()
    {
        var aggregate = Aggregate();

        _service
            .When(z => z.ValidateAggregateUpdate(aggregate, Arg.Any<Guid>()))
            .Do(_ => throw new OrganizationAccessDeniedException());

        await Assert.ThrowsAsync<OrganizationAccessDeniedException>(() =>
            UpdateOwnProfileHandler.Handle(
                Command(),
                aggregate,
                _service,
                TestClock,
                CancellationToken.None
            )
        );
    }

    private static UpdateOwnProfile Command(
        string firstName = "Johnny",
        string lastName = "Doe",
        string jobTitle = "Lead Recruiter",
        string phone = "+48 600 100 200"
    )
    {
        return new UpdateOwnProfile(
            UserGuid,
            OrganizationId.From(OrganizationGuid),
            firstName,
            lastName,
            jobTitle,
            phone
        );
    }

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
                Self,
                new ContactPerson(AggregateEmail, "John", "Doe", "Recruiter", "+48 600 100 200"),
                DateTimeOffset.UtcNow
            )
        );

        return user;
    }
}
