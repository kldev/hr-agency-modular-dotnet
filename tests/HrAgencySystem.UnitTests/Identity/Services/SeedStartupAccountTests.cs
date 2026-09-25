using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Identity.Services;

public class SeedStartupAccountTests : BaseTest
{
    private const string OwnerEmail = "owner@hr-agency.com";
    private const string OwnerPassword = "Password123!";

    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IOwnerEmailReservationRepository _reservations =
        Substitute.For<IOwnerEmailReservationRepository>();

    [Fact]
    public async Task Without_both_variables_nothing_is_created()
    {
        await Seeder(email: OwnerEmail, password: null).SeedAsync(CancellationToken.None);

        _session.Events.DidNotReceive().StartStream<PlatformOwner>(Arg.Any<Guid>(), Arg.Any<object>());
        await _session.DidNotReceiveWithAnyArgs().SaveChangesAsync();
    }

    [Fact]
    public async Task A_taken_address_is_left_alone_so_a_restart_does_not_fail()
    {
        _reservations.ExistAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        await Seeder(OwnerEmail, OwnerPassword).SeedAsync(CancellationToken.None);

        _session.Events.DidNotReceive().StartStream<PlatformOwner>(Arg.Any<Guid>(), Arg.Any<object>());
        await _session.DidNotReceiveWithAnyArgs().SaveChangesAsync();
    }

    [Fact]
    public async Task A_free_address_becomes_a_platform_owner()
    {
        _hasher.Hash(OwnerPassword).Returns("hashed");

        await Seeder(OwnerEmail, OwnerPassword).SeedAsync(CancellationToken.None);

        _session
            .Events.Received(1)
            .StartStream<PlatformOwner>(
                Arg.Any<Guid>(),
                Arg.Is<PlatformOwnerCreated>(created =>
                    created.Email == OwnerEmail
                    && created.Role == PlatformRole.Owner
                    && created.PasswordHash == "hashed"
                )
            );
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private SeedStartupAccount Seeder(string? email, string? password) =>
        new(
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        [SeedStartupAccount.EmailKey] = email,
                        [SeedStartupAccount.PasswordKey] = password,
                    }
                )
                .Build(),
            _session,
            TestClock,
            _hasher,
            _reservations,
            NullLogger<SeedStartupAccount>.Instance
        );
}
