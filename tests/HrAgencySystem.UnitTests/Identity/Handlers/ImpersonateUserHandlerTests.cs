using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Impersonate;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public class ImpersonateUserHandlerTests : BaseTest
{
    private readonly IUserQueryRepository _users = Substitute.For<IUserQueryRepository>();
    private readonly IJwtTokenService _tokens = Substitute.For<IJwtTokenService>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private static readonly Guid OrganizationGuid = Guid.NewGuid();
    private static readonly Guid AdminGuid = Guid.NewGuid();
    private static readonly Guid TargetGuid = Guid.NewGuid();

    [Fact]
    public async Task Handle_WithAMemberOfTheSameOrganization_HandsBackATokenForThem()
    {
        var target = Target();
        var expiresAt = new DateTimeOffset(2026, 9, 22, 10, 30, 0, TimeSpan.Zero);

        _users
            .GetUser(Arg.Any<OrganizationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(target);

        _tokens
            .GenerateImpersonationToken(target, AdminGuid)
            .Returns(new AccessToken("the-token", expiresAt));

        var result = await ImpersonateUserHandler.Handle(
            Command(),
            _users,
            _tokens,
            _logger,
            CancellationToken.None
        );

        Assert.Equal("the-token", result.Token);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal(TargetGuid, result.ImpersonatedUserId);
        Assert.Equal(AdminGuid, result.ImpersonatedBy);
    }

    /// <summary>
    /// Refused before the lookup: it is a question about the request, not about the organization,
    /// and a pointless attempt has no business showing up in the audit line either.
    /// </summary>
    [Fact]
    public async Task Handle_WithTheAdministratorThemselves_IsRefused()
    {
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ImpersonateUserHandler.Handle(
                Command(AdminGuid),
                _users,
                _tokens,
                _logger,
                CancellationToken.None
            )
        );

        Assert.Equal(ImpersonateUserHandler.CannotImpersonateSelfMessage, exception.Message);

        await _users
            .DidNotReceive()
            .GetUser(Arg.Any<OrganizationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// One case, not three: the repository is scoped by organization and drops the System role, so
    /// a stranger's id, a made-up id and the platform account are indistinguishable from here - and
    /// that is the point, because telling them apart would confirm which one it was.
    /// </summary>
    [Fact]
    public async Task Handle_WithSomebodyTheOrganizationCannotSee_IsNotFound()
    {
        _users
            .GetUser(Arg.Any<OrganizationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((UserProjection?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ImpersonateUserHandler.Handle(
                Command(),
                _users,
                _tokens,
                _logger,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Handle_LeavesAWarningNamingTheAdministratorAndTheTarget()
    {
        _users
            .GetUser(Arg.Any<OrganizationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Target());

        _tokens
            .GenerateImpersonationToken(Arg.Any<UserProjection>(), Arg.Any<Guid>())
            .Returns(new AccessToken("the-token", DateTimeOffset.UtcNow));

        await ImpersonateUserHandler.Handle(
            Command(),
            _users,
            _tokens,
            _logger,
            CancellationToken.None
        );

        _logger
            .Received(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    private static ImpersonateUser Command(Guid? targetUserId = null) =>
        new(targetUserId ?? TargetGuid, OrganizationId.From(OrganizationGuid), AdminGuid);

    private static UserProjection Target() =>
        new(
            TargetGuid,
            OrganizationGuid,
            "bob.smith@hr-agency.com",
            "Bob",
            "Smith",
            OrganizationRole.Recruiter,
            Guid.NewGuid(),
            new UserSnapshot(Guid.NewGuid(), "Ann", "Boss", "ann.boss@hr-agency.com"),
            DateTimeOffset.UtcNow,
            new OrganizationInfo(OrganizationGuid, "hr-agency", "HR Agency")
        );
}
