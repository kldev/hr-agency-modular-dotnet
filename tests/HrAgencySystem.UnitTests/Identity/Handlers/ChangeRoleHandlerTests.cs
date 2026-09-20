using HrAgencySystem.Identity.Application.Users.ChangeRole;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.Web.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class ChangeRoleHandlerTests : BaseTest
{
    private readonly IIdentityService _service = Substitute.For<IIdentityService>();

    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private static readonly Guid UserGuid = Guid.NewGuid();

    private static UserSnapshot Editor { get; } =
        new(Guid.NewGuid(), "Alice", "Wells", "alice-wells@hr-agency.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsRoleChanged()
    {
        var now = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);
        var aggregate = Aggregate();
        var command = Command(OrganizationRole.HiringManager);

        _service.GetUserAsync(command.ModifiedBy, Arg.Any<CancellationToken>()).Returns(Editor);

        var (@event, events) = await ChangeRoleHandler.Handle(
            command,
            aggregate,
            _service,
            new FixedClock(now),
            CancellationToken.None
        );

        Assert.Equal(UserGuid, @event.UserId);
        Assert.Equal(OrganizationGuid, @event.OrganizationId);
        Assert.Equal(OrganizationRole.Recruiter, @event.PreviousRole);
        Assert.Equal(OrganizationRole.HiringManager, @event.Role);
        Assert.Equal(Editor, @event.ModifiedBy);
        Assert.Equal(now, @event.ModifiedAt);

        Assert.Equal(@event, Assert.Single(events));

        _service.Received(1).ValidateAggregateUpdate(aggregate, OrganizationGuid);
    }

    /// <summary>
    /// The aggregate has to end up on the new role, otherwise a second change would still compare
    /// against the role the person was created with.
    /// </summary>
    [Fact]
    public async Task Handle_WithValidCommand_MovesTheAggregateToTheNewRole()
    {
        var aggregate = Aggregate();
        var command = Command(OrganizationRole.Sales);

        _service.GetUserAsync(command.ModifiedBy, Arg.Any<CancellationToken>()).Returns(Editor);

        var (@event, _) = await ChangeRoleHandler.Handle(
            command,
            aggregate,
            _service,
            TestClock,
            CancellationToken.None
        );

        aggregate.Apply(@event);

        Assert.Equal(OrganizationRole.Sales, aggregate.Role);
    }

    [Fact]
    public async Task Handle_WithTheRoleThePersonAlreadyHas_ThrowsBusinessRuleException()
    {
        var aggregate = Aggregate();
        var command = Command(OrganizationRole.Recruiter);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command, aggregate)
        );

        Assert.Equal(ChangeRoleHandler.SameRoleMessage, exception.Message);

        await _service.DidNotReceive().GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutModifiedBy_UsesSystemSnapshot()
    {
        var aggregate = Aggregate();
        var command = Command(OrganizationRole.Admin, modifiedBy: Guid.Empty);

        var (@event, _) = await ChangeRoleHandler.Handle(
            command,
            aggregate,
            _service,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(UserSnapshot.System.Email, @event.ModifiedBy.Email);

        await _service.DidNotReceive().GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAggregateBelongsToAnotherOrganization_DoesNotChangeTheRole()
    {
        var aggregate = Aggregate();
        var command = Command(OrganizationRole.Admin);

        _service
            .When(z => z.ValidateAggregateUpdate(aggregate, OrganizationGuid))
            .Throw(new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage));

        await Assert.ThrowsAsync<BusinessRuleException>(() => HandleCommand(command, aggregate));

        await _service.DidNotReceive().GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static ChangeRole Command(OrganizationRole role, Guid? modifiedBy = null) =>
        new(UserGuid, OrganizationId.From(OrganizationGuid), role, modifiedBy ?? Guid.NewGuid());

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

    private async Task HandleCommand(ChangeRole command, User aggregate)
    {
        await ChangeRoleHandler.Handle(
            command,
            aggregate,
            _service,
            TestClock,
            CancellationToken.None
        );
    }
}
