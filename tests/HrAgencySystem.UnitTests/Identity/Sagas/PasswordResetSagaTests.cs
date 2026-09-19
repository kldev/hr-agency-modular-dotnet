using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Sagas;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using NSubstitute;
using UserIdentity = HrAgencySystem.Identity.Domain.ValueObjects.UserId;

namespace HrAgencySystem.UnitTests.Identity.Sagas;

public sealed class PasswordResetSagaTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();

    private readonly IUserEmailReservationRepository _reservations =
        Substitute.For<IUserEmailReservationRepository>();

    private readonly IRefreshTokenRepository _refreshTokens =
        Substitute.For<IRefreshTokenRepository>();

    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();

    private static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    private static readonly Guid ResetId = Guid.NewGuid();
    private static readonly Guid UserGuid = Guid.NewGuid();
    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private const string Token = "a-token-from-the-mail";
    private const string NewPassword = "N3wPassword!";
    private const int ExpiresInMinutes = 15;

    [Fact]
    public void Start_SendsTheMailAndSchedulesTheEndOfTheWindow()
    {
        var (saga, mail, timeout) = PasswordResetSaga.Start(StartMessage(), new FixedClock(Now));

        Assert.Equal(ResetId, saga.Id);
        Assert.Equal(UserGuid, saga.UserId);
        Assert.Equal(PasswordResetSaga.Hash(Token), saga.TokenHash);
        Assert.Equal(Now.AddMinutes(ExpiresInMinutes), saga.ExpiresAt);

        Assert.Equal("bob.smith@hr-agency.com", mail.RecipientEmail);
        Assert.Equal(ExpiresInMinutes, mail.ExpiresInMinutes);
        Assert.Equal("http://localhost:4300/reset-password?id=x&token=y", mail.ResetUrl);

        Assert.Equal(ResetId, timeout.ResetId);
        Assert.Equal(TimeSpan.FromMinutes(ExpiresInMinutes), timeout.DelayTime);
    }

    [Fact]
    public void Handle_WhenTheWindowRunsOut_CompletesTheSaga()
    {
        var saga = Saga();

        saga.Handle(new PasswordResetExpired(ResetId, TimeSpan.FromMinutes(ExpiresInMinutes)));

        Assert.True(saga.IsCompleted());
    }

    [Fact]
    public async Task Handle_WithTheRightToken_ChangesThePasswordAndClosesTheWindow()
    {
        var saga = Saga();

        _hasher.Hash(NewPassword).Returns("new-hash");
        _identity
            .GetUserAsync(UserGuid, Arg.Any<CancellationToken>())
            .Returns(new UserSnapshot(UserGuid, "Bob", "Smith", "bob.smith@hr-agency.com"));

        await Handle(saga, new CompletePasswordReset(ResetId, Token, NewPassword));

        await _reservations
            .Received(1)
            .ChangePasswordAsync(
                OrganizationId.From(OrganizationGuid),
                UserIdentity.From(UserGuid),
                "new-hash",
                Arg.Any<CancellationToken>()
            );

        // whoever forced the reset is the reason for it, so the sessions opened before it die too
        await _refreshTokens
            .Received(1)
            .RevokeUserSessionsAsync(
                OrganizationId.From(OrganizationGuid),
                UserIdentity.From(UserGuid),
                Arg.Any<CancellationToken>()
            );

        Assert.True(saga.IsCompleted());
    }

    [Fact]
    public async Task Handle_WithAForgedToken_ChangesNothing()
    {
        var saga = Saga();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(saga, new CompletePasswordReset(ResetId, "not-the-token", NewPassword))
        );

        Assert.Equal(PasswordResetSaga.InvalidTokenMessage, exception.Message);
        Assert.False(saga.IsCompleted());
        await NothingChanged();
    }

    [Fact]
    public async Task Handle_AfterTheWindowClosed_ChangesNothing()
    {
        var saga = Saga();

        // The timeout can lose a race with a redelivered command; the saga still refuses.
        var late = new FixedClock(Now.AddMinutes(ExpiresInMinutes + 1));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(saga, new CompletePasswordReset(ResetId, Token, NewPassword), late)
        );

        await NothingChanged();
    }

    private async Task NothingChanged()
    {
        await _reservations
            .DidNotReceive()
            .ChangePasswordAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserIdentity>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await _refreshTokens
            .DidNotReceive()
            .RevokeUserSessionsAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<UserIdentity>(),
                Arg.Any<CancellationToken>()
            );
    }

    private Task Handle(
        PasswordResetSaga saga,
        CompletePasswordReset message,
        IClock? clock = null
    ) =>
        saga.Handle(
            message,
            _identity,
            _reservations,
            _refreshTokens,
            _hasher,
            _session,
            clock ?? new FixedClock(Now),
            CancellationToken.None
        );

    private static StartPasswordReset StartMessage() =>
        new(
            ResetId,
            UserGuid,
            OrganizationGuid,
            "bob.smith@hr-agency.com",
            "Bob Smith",
            PasswordResetSaga.Hash(Token),
            "http://localhost:4300/reset-password?id=x&token=y",
            ExpiresInMinutes
        );

    private static PasswordResetSaga Saga() =>
        new()
        {
            Id = ResetId,
            UserId = UserGuid,
            OrganizationId = OrganizationGuid,
            TokenHash = PasswordResetSaga.Hash(Token),
            ExpiresAt = Now.AddMinutes(ExpiresInMinutes),
        };
}
