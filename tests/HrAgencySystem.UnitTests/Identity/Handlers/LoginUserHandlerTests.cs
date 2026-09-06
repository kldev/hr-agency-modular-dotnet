using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Handlers;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public sealed class LoginUserHandlerTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly IJwtTokenService _tokenService = Substitute.For<IJwtTokenService>();
    private readonly IOrganizationService _organizationService = Substitute.For<IOrganizationService>();

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUser(
            "john@example.com",
            "password",
            "acme");

        var reservation = CreateReservation(
            passwordHash: "hashed-password");

        var user = CreateUser();

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher
            .Matches("password", "hashed-password")
            .Returns(true);

        _repository
            .GetUser(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        _tokenService
            .GenerateUserToken(user)
            .Returns("jwt-token");

        // Act
        var result = await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            CancellationToken.None);

        // Assert
        Assert.Equal("jwt-token", result.Token);

        _tokenService.Received(1).GenerateUserToken(user);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthorizationException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginUser(
            "john@example.com",
            "password",
            "acme");

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                Arg.Any<CancellationToken>())
            .Returns((UserEmailReservation?)null);

        // Act
        var action = () => LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(action);

        Assert.Equal(
            "Invalid login or password",
            exception.Message);

        _hasher.DidNotReceiveWithAnyArgs().Matches(default!, default!);
        _repository.DidNotReceive().GetUser(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>());

        _tokenService.DidNotReceiveWithAnyArgs()
            .GenerateUserToken(default!);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthorizationException_WhenPasswordIsInvalid()
    {
        // Arrange
        var command = new LoginUser(
            "john@example.com",
            "wrong-password",
            "acme");

        var reservation = CreateReservation(
            passwordHash: "hashed-password");

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher
            .Matches("wrong-password", "hashed-password")
            .Returns(false);

        // Act
        var action = () => LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(action);

        Assert.Equal(
            "Invalid login or password",
            exception.Message);

        _repository.DidNotReceive().GetUser(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>());

        _tokenService.DidNotReceiveWithAnyArgs()
            .GenerateUserToken(default!);
    }

    [Fact]
    public async Task Handle_ShouldUseProvidedSlug_WhenSlugIsSpecified()
    {
        // Arrange
        var command = new LoginUser(
            "john@example.com",
            "password",
            "acme");

        var reservation = CreateReservation();
        var user = CreateUser();

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher
            .Matches(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        _repository
            .GetUser(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        _tokenService
            .GenerateUserToken(user)
            .Returns("jwt-token");

        // Act
        await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            CancellationToken.None);

        // Assert
        await _repository.Received(1).FindUserByEmail(
            Arg.Is<Email>(x => x.Value == "john@example.com"),
            "acme",
            Arg.Any<CancellationToken>());

        await _organizationService
            .DidNotReceive()
            .GetByEmailDomainAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldResolveOrganizationByEmailDomain_WhenSlugIsEmpty()
    {
        // Arrange
        var command = new LoginUser(
            "john@example.com",
            "password",
            "");

        var reservation = CreateReservation();
        var user = CreateUser();

        var organization = new OrganizationInfo(Guid.NewGuid(), "acme", "Name");

        
        _organizationService
            .GetByEmailDomainAsync(
                "example.com",
                Arg.Any<CancellationToken>())
            .Returns(organization);

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher
            .Matches(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        _repository
            .GetUser(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        _tokenService
            .GenerateUserToken(user)
            .Returns("jwt-token");

        // Act
        var result = await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            CancellationToken.None);

        // Assert
        Assert.Equal("jwt-token", result.Token);

        await _organizationService.Received(1)
            .GetByEmailDomainAsync(
                "example.com",
                Arg.Any<CancellationToken>());

        await _repository.Received(1)
            .FindUserByEmail(
                Arg.Is<Email>(x => x.Value == "john@example.com"),
                "acme",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseEmptySlug_WhenOrganizationCannotBeResolved()
    {
        // Arrange
        var command = new LoginUser(
            "john@example.com",
            "password",
            "");

        var reservation = CreateReservation();
        var user = CreateUser();

        _organizationService
            .GetByEmailDomainAsync(
                "example.com",
                Arg.Any<CancellationToken>())
            .Returns((OrganizationInfo?)null);

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "",
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _hasher
            .Matches(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        _repository
            .GetUser(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        _tokenService
            .GenerateUserToken(user)
            .Returns("jwt-token");

        // Act
        var result = await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            CancellationToken.None);

        // Assert
        Assert.Equal("jwt-token", result.Token);

        await _repository.Received(1)
            .FindUserByEmail(
                Arg.Any<Email>(),
                "",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToDependencies()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var command = new LoginUser(
            "john@example.com",
            "password",
            "acme");

        var reservation = CreateReservation();
        var user = CreateUser();

        _repository
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                cancellationToken)
            .Returns(reservation);

        _hasher
            .Matches(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        _repository
            .GetUser(
                Arg.Any<UserId>(),
                cancellationToken)
            .Returns(user);

        _tokenService
            .GenerateUserToken(user)
            .Returns("jwt-token");

        // Act
        await LoginUserHandler.Handle(
            command,
            _logger,
            _hasher,
            _repository,
            _tokenService,
            _organizationService,
            cancellationToken);

        // Assert
        await _repository.Received(1)
            .FindUserByEmail(
                Arg.Any<Email>(),
                "acme",
                cancellationToken);

        await _repository.Received(1)
            .GetUser(
                Arg.Any<UserId>(),
                cancellationToken);
    }

    private static UserEmailReservation CreateReservation(
        string passwordHash = "hashed-password")
    {
        return new UserEmailReservation(
            Id: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Email: "john@example.com",
            PasswordHash: passwordHash);
    }

    private static UserProjection CreateUser()
    {
        return new UserProjection(Guid.NewGuid(), Guid.NewGuid(), "joe@test.io", "Joe", "Test",
            OrganizationRole.Recruiter, Guid.NewGuid(),
            new UserSnapshot(Guid.NewGuid(), "", "", ""), DateTimeOffset.UtcNow);
    }
}