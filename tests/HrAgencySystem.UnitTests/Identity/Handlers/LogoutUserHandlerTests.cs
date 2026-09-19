using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Logout;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public sealed class LogoutUserHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens =
        Substitute.For<IRefreshTokenRepository>();

    private static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithAKnownToken_RevokesTheWholeFamily()
    {
        var (stored, value) = RefreshToken.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new FixedClock(Now),
            30
        );

        _refreshTokens
            .FindByHashAsync(SecureToken.Hash(value), Arg.Any<CancellationToken>())
            .Returns(stored);

        await LogoutUserHandler.Handle(new LogoutUser(value), _refreshTokens, default);

        // not just this token: a rotated-away sibling would otherwise survive the logout
        await _refreshTokens
            .Received(1)
            .RevokeFamilyAsync(stored.FamilyId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAnUnknownToken_DoesNothingAndDoesNotComplain()
    {
        _refreshTokens
            .FindByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        await LogoutUserHandler.Handle(new LogoutUser("never-issued"), _refreshTokens, default);

        await _refreshTokens
            .DidNotReceive()
            .RevokeFamilyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
