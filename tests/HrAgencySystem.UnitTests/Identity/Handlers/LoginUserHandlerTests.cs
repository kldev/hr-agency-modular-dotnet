using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Login;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public sealed class LoginUserHandlerTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly IJwtTokenService _tokenService = Substitute.For<IJwtTokenService>();
    private readonly IQueryOrganizationRepository _queryOrganizationRepository =
        Substitute.For<IQueryOrganizationRepository>();
    private readonly IRefreshTokenRepository _refreshTokens =
        Substitute.For<IRefreshTokenRepository>();

    private static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);
    private static readonly IClock Clock = new FixedClock(Now);

    private static readonly IOptions<JwtConfig> Jwt = Options.Create(
        new JwtConfig { ExpiresInHours = 6, RefreshTokenExpiresInDays = 30 }
    );

    private static readonly AccessToken Access = new("jwt-token", Now.AddHours(6));

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "acme");

        var reservation = CreateReservation(passwordHash: "hashed-password");

        var user = CreateUser();

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "acme", Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher.Matches("password", "hashed-password").Returns(true);

        _repository.GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        _tokenService.GenerateUserToken(user).Returns(Access);

        // Act
        var result = await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _queryOrganizationRepository,
            _refreshTokens,
            Jwt,
            Clock,
            CancellationToken.None
        );

        // Assert
        Assert.Equal("jwt-token", result.Token);

        _tokenService.Received(1).GenerateUserToken(user);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthorizationException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "acme");

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "acme", Arg.Any<CancellationToken>())
            .Returns((UserEmailReservation?)null);

        // Act
        var action = () =>
            LoginUserHandler.Handle(
                command,
                _logger,
                _hasher,
                _repository,
                _tokenService,
                _queryOrganizationRepository,
                _refreshTokens,
                Jwt,
                Clock,
                CancellationToken.None
            );

        // Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(action);

        Assert.Equal("Invalid login or password", exception.Message);

        _hasher.DidNotReceiveWithAnyArgs().Matches(default!, default!);
        await _repository.DidNotReceive().GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>());

        _tokenService.DidNotReceiveWithAnyArgs().GenerateUserToken(null!);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthorizationException_WhenPasswordIsInvalid()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "wrong-password", "acme");

        var reservation = CreateReservation(passwordHash: "hashed-password");

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "acme", Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher.Matches("wrong-password", "hashed-password").Returns(false);

        // Act
        var action = () =>
            LoginUserHandler.Handle(
                command,
                _logger,
                _hasher,
                _repository,
                _tokenService,
                _queryOrganizationRepository,
                _refreshTokens,
                Jwt,
                Clock,
                CancellationToken.None
            );

        // Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(action);

        Assert.Equal("Invalid login or password", exception.Message);

        await _repository.DidNotReceive().GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>());

        _tokenService.DidNotReceiveWithAnyArgs().GenerateUserToken(null!);
    }

    [Fact]
    public async Task Handle_ShouldUseProvidedSlug_WhenSlugIsSpecified()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "acme");

        var reservation = CreateReservation();
        var user = CreateUser();

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "acme", Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher.Matches(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        _tokenService.GenerateUserToken(user).Returns(Access);

        // Act
        await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _queryOrganizationRepository,
            _refreshTokens,
            Jwt,
            Clock,
            CancellationToken.None
        );

        // Assert
        await _repository
            .Received(1)
            .FindUserByEmail(
                Arg.Is<Email>(x => x.Value == "john@example.com"),
                "acme",
                Arg.Any<CancellationToken>()
            );

        await _queryOrganizationRepository
            .DidNotReceive()
            .GetByEmailDomainAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldResolveOrganizationByEmailDomain_WhenSlugIsEmpty()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "");

        var reservation = CreateReservation();
        var user = CreateUser();

        var organization = new OrganizationInfo(Guid.NewGuid(), "acme", "Name");

        _queryOrganizationRepository
            .GetByEmailDomainAsync("example.com", Arg.Any<CancellationToken>())
            .Returns(organization);

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "acme", Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher.Matches(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        _tokenService.GenerateUserToken(user).Returns(Access);

        // Act
        var result = await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _queryOrganizationRepository,
            _refreshTokens,
            Jwt,
            Clock,
            CancellationToken.None
        );

        // Assert
        Assert.Equal("jwt-token", result.Token);

        await _queryOrganizationRepository
            .Received(1)
            .GetByEmailDomainAsync("example.com", Arg.Any<CancellationToken>());

        await _repository
            .Received(1)
            .FindUserByEmail(
                Arg.Is<Email>(x => x.Value == "john@example.com"),
                "acme",
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task? Handle_Should_ThrowException_WhenOrganizationNotResolvedByEmailDomain()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "");

        var reservation = CreateReservation();
        var user = CreateUser();

        _queryOrganizationRepository
            .GetByEmailDomainAsync("example.com", Arg.Any<CancellationToken>())
            .Returns((OrganizationInfo?)null);

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "", Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher.Matches(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        _tokenService.GenerateUserToken(user).Returns(Access);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await LoginUserHandler.Handle(
                command,
                _logger,
                _hasher,
                _repository,
                _tokenService,
                _queryOrganizationRepository,
                _refreshTokens,
                Jwt,
                Clock,
                CancellationToken.None
            );
        });

        Assert.Equal("Organization by domain not found by example.com", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToDependencies()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var command = new LoginUser("john@example.com", "password", "acme");

        var reservation = CreateReservation();
        var user = CreateUser();

        _repository
            .FindUserByEmail(Arg.Any<Email>(), "acme", cancellationToken)
            .Returns(reservation);

        _hasher.Matches(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetUser(Arg.Any<UserId>(), cancellationToken).Returns(user);

        _tokenService.GenerateUserToken(user).Returns(Access);

        // Act
        await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _queryOrganizationRepository,
            _refreshTokens,
            Jwt,
            Clock,
            cancellationToken
        );

        // Assert
        await _repository.Received(1).FindUserByEmail(Arg.Any<Email>(), "acme", cancellationToken);

        await _repository.Received(1).GetUser(Arg.Any<UserId>(), cancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldIssueARefreshTokenStoredOnlyAsAHash_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "acme");

        ValidCredentials(CreateUser());

        // Act
        var result = await Login(command);

        // Assert
        var issued = IssuedRefreshToken();

        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEqual(result.RefreshToken, issued.TokenHash);
        Assert.Equal(SecureToken.Hash(result.RefreshToken), issued.TokenHash);
    }

    [Fact]
    public async Task Handle_ShouldExpireTheRefreshTokenAfterTheConfiguredDays()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "acme");

        ValidCredentials(CreateUser());

        // Act
        var result = await Login(command);

        // Assert
        var issued = IssuedRefreshToken();

        Assert.Equal(Now.AddDays(30), issued.ExpiresAt);
        Assert.Equal(Now.AddDays(30), result.RefreshTokenExpiresAt);
        Assert.Equal(Access.ExpiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task Handle_ShouldOpenAFreshFamilyForEveryLogin()
    {
        // Arrange
        var command = new LoginUser("john@example.com", "password", "acme");

        ValidCredentials(CreateUser());

        // Act
        await Login(command);
        await Login(command);

        // Assert
        var issued = IssuedRefreshTokens();

        // signing in on a second device must not disturb the first one
        Assert.Equal(2, issued.Count);
        Assert.NotEqual(issued[0].FamilyId, issued[1].FamilyId);
        Assert.All(issued, token => Assert.Equal(token.Id, token.FamilyId));
        Assert.All(issued, token => Assert.Null(token.UsedAt));
    }

    private Task<LoginUserResult> Login(LoginUser command) =>
        LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _queryOrganizationRepository,
            _refreshTokens,
            Jwt,
            Clock,
            CancellationToken.None
        );

    private void ValidCredentials(UserProjection user)
    {
        _repository
            .FindUserByEmail(Arg.Any<Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(CreateReservation());

        _hasher.Matches(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetUser(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        _tokenService.GenerateUserToken(user).Returns(Access);
    }

    private RefreshToken IssuedRefreshToken() => Assert.Single(IssuedRefreshTokens());

    private List<RefreshToken> IssuedRefreshTokens() =>
        _refreshTokens
            .ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(IRefreshTokenRepository.IssueAsync))
            .Select(call => (RefreshToken)call.GetArguments()[0]!)
            .ToList();

    private static UserEmailReservation CreateReservation(string passwordHash = "hashed-password")
    {
        return new UserEmailReservation(
            Id: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Email: "john@example.com",
            PasswordHash: passwordHash
        );
    }

    private static UserProjection CreateUser()
    {
        var info = new OrganizationInfo(Guid.NewGuid(), "hr-test", "Test");
        return new UserProjection(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "joe@test.io",
            "Joe",
            "Test",
            OrganizationRole.Recruiter,
            Guid.NewGuid(),
            new UserSnapshot(Guid.NewGuid(), "", "", ""),
            DateTimeOffset.UtcNow,
            info
        );
    }
}
