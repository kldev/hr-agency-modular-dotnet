using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.RequestPasswordReset;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Configuration;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.Identity.Sagas;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Handlers;

public sealed class RequestPasswordResetHandlerTests
{
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();

    private readonly IQueryOrganizationRepository _organizations =
        Substitute.For<IQueryOrganizationRepository>();

    private static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    private static readonly Guid OrganizationGuid = Guid.NewGuid();
    private static readonly Guid UserGuid = Guid.NewGuid();

    private const string Email = "bob.smith@hr-agency.com";
    private const string PortalUrl = "http://localhost:4300";
    private const int ExpiresInMinutes = 20;

    [Fact]
    public async Task Handle_WithKnownAddress_StartsTheResetWindow()
    {
        KnownUser();

        var (_, messages) = await Handle(new RequestPasswordReset(Email, "hr-agency", PortalUrl));

        var start = Assert.Single(messages.OfType<StartPasswordReset>());

        Assert.Equal(UserGuid, start.UserId);
        Assert.Equal(OrganizationGuid, start.OrganizationId);
        Assert.Equal(Email, start.RecipientEmail);
        Assert.Equal("Bob Smith", start.RecipientFullname);
        Assert.Equal(ExpiresInMinutes, start.ExpiresInMinutes);
        Assert.StartsWith($"{PortalUrl}/reset-password?id={start.ResetId}&token=", start.ResetUrl);
    }

    [Fact]
    public async Task Handle_WithKnownAddress_KeepsOnlyTheHashOfTheMailedToken()
    {
        KnownUser();

        var (_, messages) = await Handle(new RequestPasswordReset(Email, "hr-agency", PortalUrl));

        var start = messages.OfType<StartPasswordReset>().Single();
        var token = start.ResetUrl.Split("token=")[1];

        Assert.NotEqual(token, start.TokenHash);
        Assert.Equal(PasswordResetSaga.Hash(token), start.TokenHash);
    }

    [Fact]
    public async Task Handle_WithUnknownAddress_StartsNothing()
    {
        _accounts
            .FindUserByEmail(Arg.Any<Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((UserEmailReservation?)null);

        var (_, messages) = await Handle(new RequestPasswordReset(Email, "hr-agency", PortalUrl));

        Assert.Empty(messages);
    }

    [Fact]
    public async Task Handle_WithoutSlugAndUnknownEmailDomain_StartsNothing()
    {
        _organizations
            .GetByEmailDomainAsync("hr-agency.com", Arg.Any<CancellationToken>())
            .Returns((OrganizationInfo?)null);

        var (_, messages) = await Handle(new RequestPasswordReset(Email, "", PortalUrl));

        Assert.Empty(messages);
        await _accounts
            .DidNotReceive()
            .FindUserByEmail(Arg.Any<Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private Task<(RequestPasswordResetResult, Wolverine.OutgoingMessages)> Handle(
        RequestPasswordReset command
    ) =>
        RequestPasswordResetHandler.Handle(
            command,
            _accounts,
            _organizations,
            Options.Create(new IdentityConfig { PasswordResetExpiresInMinutes = ExpiresInMinutes }),
            new FixedClock(Now),
            NullLogger.Instance,
            CancellationToken.None
        );

    private void KnownUser()
    {
        _accounts
            .FindUserByEmail(Arg.Any<Email>(), "hr-agency", Arg.Any<CancellationToken>())
            .Returns(
                new UserEmailReservation(Guid.NewGuid(), UserGuid, OrganizationGuid, Email, "hash")
            );

        _accounts
            .GetUser(UserId.From(UserGuid), Arg.Any<CancellationToken>())
            .Returns(
                new UserProjection(
                    UserGuid,
                    OrganizationGuid,
                    Email,
                    "Bob",
                    "Smith",
                    OrganizationRole.Recruiter,
                    Guid.NewGuid(),
                    new UserSnapshot(Guid.NewGuid(), "Ann", "Boss", "ann.boss@hr-agency.com"),
                    Now,
                    new OrganizationInfo(OrganizationGuid, "hr-agency", "HR Agency")
                )
            );
    }
}
