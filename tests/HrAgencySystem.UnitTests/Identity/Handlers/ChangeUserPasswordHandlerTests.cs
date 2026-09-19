using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.ChangePassword;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.Web.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class ChangeUserPasswordHandlerTests : BaseTest
{
    private readonly IIdentityService _service = Substitute.For<IIdentityService>();

    private readonly IUserEmailReservationRepository _emailReservationRepository =
        Substitute.For<IUserEmailReservationRepository>();

    private readonly IRefreshTokenRepository _refreshTokens =
        Substitute.For<IRefreshTokenRepository>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private const string CurrentHash = "current-hash";

    private const string NewHash = "new-hash";

    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private static readonly Guid UserGuid = Guid.NewGuid();

    private static UserSnapshot Owner { get; } =
        new(UserGuid, "John", "Doe", "john.doe@example.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsPasswordChanged()
    {
        var now = new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero);
        var aggregate = Aggregate();
        var command = Command();

        MatchCurrentPassword();
        _hasher.Hash(command.NewPassword).Returns(NewHash);
        _service.GetUserAsync(UserGuid, Arg.Any<CancellationToken>()).Returns(Owner);

        var (@event, events) = await ChangeUserPasswordHandler.Handle(
            command,
            aggregate,
            _service,
            _emailReservationRepository,
            _refreshTokens,
            _hasher,
            new FixedClock(now),
            CancellationToken.None
        );

        Assert.Equal(UserGuid, @event.UserId);
        Assert.Equal(OrganizationGuid, @event.OrganizationId);
        Assert.Equal(NewHash, @event.PasswordHash);
        Assert.Equal(Owner, @event.ModifiedBy);
        Assert.Equal(now, @event.ModifiedAt);

        Assert.Equal(@event, Assert.Single(events));

        _service.Received(1).ValidateAggregateUpdate(aggregate, OrganizationGuid);

        await _emailReservationRepository
            .Received(1)
            .ChangePasswordAsync(
                Arg.Is<OrganizationId>(z => z.Value == OrganizationGuid),
                Arg.Is<UserId>(z => z.Value == UserGuid),
                NewHash,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithValidCommand_EndsEveryOpenSession()
    {
        var command = Command();

        MatchCurrentPassword();
        _hasher.Hash(command.NewPassword).Returns(NewHash);
        _service.GetUserAsync(UserGuid, Arg.Any<CancellationToken>()).Returns(Owner);

        await HandleCommand(command);

        // a refresh token taken together with the old password must not outlive it
        await _refreshTokens
            .Received(1)
            .RevokeUserSessionsAsync(
                Arg.Is<OrganizationId>(z => z.Value == OrganizationGuid),
                Arg.Is<UserId>(z => z.Value == UserGuid),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithWrongCurrentPassword_ThrowsBusinessRuleException()
    {
        var command = Command(currentPassword: "not-my-password");

        _hasher.Matches(command.CurrentPassword, CurrentHash).Returns(false);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command)
        );

        Assert.Equal(ChangeUserPasswordHandler.InvalidCurrentPasswordMessage, exception.Message);

        await AssertPasswordNotChanged();
    }

    [Fact]
    public async Task Handle_WithPasswordBelowPolicy_ThrowsBusinessRuleException()
    {
        var command = Command(newPassword: "abc");

        MatchCurrentPassword();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command)
        );

        Assert.Equal(PasswordPolicyValidator.InvalidPasswordMessage, exception.Message);

        await AssertPasswordNotChanged();
    }

    [Fact]
    public async Task Handle_WithTheSamePassword_ThrowsBusinessRuleException()
    {
        var command = Command(newPassword: "Password123!");

        MatchCurrentPassword();
        _hasher.Matches(command.NewPassword, CurrentHash).Returns(true);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            HandleCommand(command)
        );

        Assert.Equal(ChangeUserPasswordHandler.SamePasswordMessage, exception.Message);

        await AssertPasswordNotChanged();
    }

    [Fact]
    public async Task Handle_WhenAggregateBelongsToAnotherOrganization_DoesNotChangePassword()
    {
        var aggregate = Aggregate();
        var command = Command();

        _service
            .When(z => z.ValidateAggregateUpdate(aggregate, OrganizationGuid))
            .Throw(new OrganizationAccessDeniedException());

        await Assert.ThrowsAsync<OrganizationAccessDeniedException>(() =>
            HandleCommand(command, aggregate)
        );

        await AssertPasswordNotChanged();
    }

    private void MatchCurrentPassword()
    {
        _hasher.Matches("Password123!", CurrentHash).Returns(true);
    }

    private async Task AssertPasswordNotChanged()
    {
        await _emailReservationRepository
            .DidNotReceive()
            .ChangePasswordAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserId>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );
    }

    private static ChangeUserPassword Command(
        string currentPassword = "Password123!",
        string newPassword = "NewPassword123!"
    ) => new(UserGuid, OrganizationId.From(OrganizationGuid), currentPassword, newPassword);

    private static User Aggregate()
    {
        var user = User.Empty();

        user.Apply(
            new UserCreated(
                UserGuid,
                OrganizationGuid,
                OrganizationRole.Recruiter,
                CurrentHash,
                new OrganizationInfo(OrganizationGuid, "hr-agency", "HR Agency"),
                Owner,
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

    private async Task HandleCommand(ChangeUserPassword command, User? aggregate = null)
    {
        await ChangeUserPasswordHandler.Handle(
            command,
            aggregate ?? Aggregate(),
            _service,
            _emailReservationRepository,
            _refreshTokens,
            _hasher,
            TestClock,
            CancellationToken.None
        );
    }
}
