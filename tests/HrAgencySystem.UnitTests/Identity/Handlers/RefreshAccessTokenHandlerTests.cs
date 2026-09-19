using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Login;
using HrAgencySystem.Identity.Application.Users.Refresh;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public sealed class RefreshAccessTokenHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens =
        Substitute.For<IRefreshTokenRepository>();

    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly IJwtTokenService _tokenService = Substitute.For<IJwtTokenService>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();

    private static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    private static readonly Guid UserGuid = Guid.NewGuid();
    private static readonly Guid OrganizationGuid = Guid.NewGuid();

    private const int ExpiresInDays = 30;

    private static readonly AccessToken Access = new("fresh-jwt", Now.AddHours(6));

    public RefreshAccessTokenHandlerTests()
    {
        _accounts.GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(User());
        _tokenService.GenerateUserToken(Arg.Any<UserProjection>()).Returns(Access);
    }

    [Fact]
    public async Task Handle_WithALiveToken_ReturnsANewPair()
    {
        var (stored, value) = Issued();

        Holds(stored, value);

        var result = await Handle(value);

        Assert.Equal(Access.Value, result.Token);
        Assert.Equal(Access.ExpiresAt, result.ExpiresAt);
        Assert.NotEqual(value, result.RefreshToken);
    }

    [Fact]
    public async Task Handle_WithALiveToken_KeepsTheExpiryOfTheLogin()
    {
        var (stored, value) = Issued();

        Holds(stored, value);

        // eleven days into the session, so a sliding window would be visible in the result
        var result = await Handle(value, new FixedClock(Now.AddDays(11)));

        var (spent, replacement) = Rotation();

        Assert.Equal(stored.ExpiresAt, replacement.ExpiresAt);
        Assert.Equal(Now.AddDays(ExpiresInDays), result.RefreshTokenExpiresAt);
        Assert.Equal(stored.FamilyId, replacement.FamilyId);
        Assert.Equal(spent.Id, stored.Id);
    }

    [Fact]
    public async Task Handle_WithALiveToken_SpendsTheOldOneAndStoresOnlyTheHashOfTheNewOne()
    {
        var (stored, value) = Issued();

        Holds(stored, value);

        var result = await Handle(value);

        var (spent, replacement) = Rotation();

        Assert.Equal(Now, spent.UsedAt);
        Assert.Equal(replacement.Id, spent.ReplacedById);
        Assert.Equal(SecureToken.Hash(result.RefreshToken), replacement.TokenHash);
        Assert.Null(replacement.UsedAt);
    }

    [Fact]
    public async Task Handle_WithAnUnknownToken_RejectsAndRevokesNothing()
    {
        _refreshTokens
            .FindByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        await Rejected("never-issued");

        await _refreshTokens
            .DidNotReceive()
            .RevokeFamilyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAnAlreadyUsedToken_RevokesTheWholeFamily()
    {
        var (stored, value) = Issued();
        var (replacement, _) = stored.Rotate(new FixedClock(Now));

        // the token was exchanged a minute ago and is being presented a second time
        Holds(stored.SpentOn(replacement, new FixedClock(Now)), value);

        await Rejected(value);

        await _refreshTokens
            .Received(1)
            .RevokeFamilyAsync(stored.FamilyId, Arg.Any<CancellationToken>());

        await _refreshTokens
            .DidNotReceive()
            .RotateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithAnExpiredToken_RevokesTheWholeFamily()
    {
        var (stored, value) = Issued();

        Holds(stored, value);

        await Rejected(value, new FixedClock(Now.AddDays(ExpiresInDays)));

        await _refreshTokens
            .Received(1)
            .RevokeFamilyAsync(stored.FamilyId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenItRevokes_CommitsBeforeThrowing()
    {
        var (stored, value) = Issued();

        Holds(stored, value);

        await Rejected(value, new FixedClock(Now.AddDays(ExpiresInDays)));

        // the exception rolls the handler transaction back, so the revocation has to be its own commit
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private async Task Rejected(string value, IClock? clock = null)
    {
        var exception = await Assert.ThrowsAsync<AuthorizationException>(() =>
            Handle(value, clock)
        );

        Assert.Equal(RefreshAccessTokenHandler.InvalidTokenMessage, exception.Message);
    }

    private (RefreshToken Spent, RefreshToken Replacement) Rotation()
    {
        var call = Assert.Single(
            _refreshTokens.ReceivedCalls(),
            x => x.GetMethodInfo().Name == nameof(IRefreshTokenRepository.RotateAsync)
        );

        var arguments = call.GetArguments();

        return ((RefreshToken)arguments[0]!, (RefreshToken)arguments[1]!);
    }

    private void Holds(RefreshToken stored, string value) =>
        _refreshTokens
            .FindByHashAsync(SecureToken.Hash(value), Arg.Any<CancellationToken>())
            .Returns(stored);

    private static (RefreshToken Token, string Value) Issued() =>
        RefreshToken.Issue(UserGuid, OrganizationGuid, new FixedClock(Now), ExpiresInDays);

    private Task<LoginUserResult> Handle(string value, IClock? clock = null) =>
        RefreshAccessTokenHandler.Handle(
            new RefreshAccessToken(value),
            NullLogger.Instance,
            _refreshTokens,
            _accounts,
            _tokenService,
            _session,
            clock ?? new FixedClock(Now),
            CancellationToken.None
        );

    private static UserProjection User() =>
        new(
            UserGuid,
            OrganizationGuid,
            "bob.smith@hr-agency.com",
            "Bob",
            "Smith",
            OrganizationRole.Recruiter,
            Guid.NewGuid(),
            new UserSnapshot(Guid.NewGuid(), "Ann", "Boss", "ann.boss@hr-agency.com"),
            Now,
            new OrganizationInfo(OrganizationGuid, "hr-agency", "HR Agency")
        );
}
